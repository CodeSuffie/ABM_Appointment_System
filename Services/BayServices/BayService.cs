using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.HubServices;
using Services.ModelServices;
using Services.TripServices;
using Settings;

namespace Services.BayServices;

public sealed class BayService(
    ModelDbContext context,
    BayShiftService bayShiftService,
    TripService tripService,
    HubService hubService,
    LoadService loadService,
    WorkService workService)
{
    public async Task<Bay> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            XSize = 1,
            YSize = 1,
            BayStatus = BayStatus.Closed,
            Hub = hub,
        };

        return bay;
    }

    // TODO: Repository
    public async Task<Bay> SelectBayByStaff(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var bays = await context.Bays
            .Where(x => x.HubId == bayStaff.Hub.Id)
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to the Hub of this BayStaff.");

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
        return bay;
    }

    // TODO: Repository
    private async Task<Trip?> GetTripForBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t=> t.Bay != null && t.BayId == bay.Id, cancellationToken);
        
        return trip;
    }
    
    // TODO: Repository
    private async Task<Hub?> GetHubForBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h=> h.Id == bay.HubId, cancellationToken);

        // if (hub == null)
        //     throw new Exception("There was no Hub assigned to this Bay.");
        
        return hub;
    }
    
    // TODO: Repository
    public async Task<IQueryable<BayShift>> GetBayShiftsForBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayId == bay.Id);

        return shifts;
    }
    
    // TODO: Repository
    public async Task<List<BayShift>> GetCurrentBayShiftsForBayAsync(Bay bay, CancellationToken cancellationToken)
    { 
        var shifts = (await GetBayShiftsForBayAsync(bay, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
    
        var currentShifts = new List<BayShift>();
    
        await foreach (var shift in shifts)
        {
            if (await bayShiftService.IsCurrentShiftAsync(shift, cancellationToken))
            {
                currentShifts.Add(shift);
            }
        }
        
        return currentShifts;
    }

    // TODO: Repository
    public async Task SetBayStatusAsync(Bay bay, BayStatus status, CancellationToken cancellationToken)
    {
        bay.BayStatus = status;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    // TODO: Repository
    public async Task RemoveTripAsync(Bay bay, Trip trip, CancellationToken cancellationToken)
    {
        trip.Bay = null;
        bay.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    // TODO: Repository
    public async Task AddTripAsync(Bay bay, Trip trip, CancellationToken cancellationToken)
    {
        // TODO: If already an active ParkingSpot or Bay, throw Exception or Log
        
        trip.Bay = bay;
        bay.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task StartDropOffAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workService.AddWorkAsync(bay, bayStaff, WorkType.DropOff, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await GetTripForBayAsync(bay, cancellationToken);
        if (trip == null)
            throw new Exception("Cannot start a Fetch Work Job for a BayStaff at a Bay that has no Trip assigned.");
        
        var pickUpLoad = await tripService.GetPickUpLoadForTripAsync(trip, cancellationToken);
        
        if (pickUpLoad != null)
        {
            var pickUpLoadBay = await loadService.GetBayForLoadAsync(pickUpLoad, cancellationToken);
            
            if (pickUpLoadBay == null)
            {
                if (bay.BayStatus == BayStatus.WaitingFetchStart)
                {
                    await AlertPickedUpAsync(bay, cancellationToken);
                    // TODO: Log Load miss
                }
                else
                {
                    // TODO: Log that Load has not arrived yet
                }
                
            }
            
            else if (pickUpLoadBay.Id != bay.Id)
            {
                await workService.AddWorkAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
                return;
            }
        }
        
        // TODO: Not fetched but done
        await AlertFetchedAsync(bay, cancellationToken);
        await AlertFreeAsync(bay, bayStaff, cancellationToken);
    }
    
    public async Task StartPickUpAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workService.AddWorkAsync(bay, bayStaff, WorkType.PickUp, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    
    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        var oldTrip = await GetTripForBayAsync(bay, cancellationToken);
        if (oldTrip != null)
        {
            await RemoveTripAsync(bay, oldTrip, cancellationToken);
        }

        var hub = await GetHubForBayAsync(bay, cancellationToken);
        if (hub == null) return;
        
        var newTrip = await hubService.GetNextBayTripAsync(hub, cancellationToken);
        if (newTrip != null)
        {
            await tripService.AlertFreeAsync(newTrip, bay, cancellationToken);
            await SetBayStatusAsync(bay, BayStatus.Claimed, cancellationToken);
        }
    }
    
    public async Task RemoveLoadAsync(Bay bay, Load load, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Remove Load from Bay
    }
    
    public async Task AddLoadAsync(Bay bay, Load load, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Add Load to Bay
    }
    
    public async Task MoveLoadAsync(Bay bay, Load load, CancellationToken cancellationToken)
    {
        var oldBay = await loadService.GetBayForLoadAsync(load, cancellationToken);
        if (oldBay == null)
            throw new Exception("This Load cannot be moved to another Bay since it is not located at a Bay");
        
        await RemoveLoadAsync(oldBay, load, cancellationToken);
        await AddLoadAsync(bay, load, cancellationToken);
    }

    public async Task AlertDroppedOffAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.DroppingOffStarted:
                await SetBayStatusAsync(bay, BayStatus.WaitingFetchStart, cancellationToken);
                break;
            case BayStatus.FetchStarted:
                await SetBayStatusAsync(bay, BayStatus.WaitingFetch, cancellationToken);
                break;
            case BayStatus.FetchFinished:
                await SetBayStatusAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            default:
                // TODO: Log the miss
                return;
        }

        var trip = await GetTripForBayAsync(bay, cancellationToken);
        if (trip == null) return;

        var dropOffLoad = await tripService.GetDropOffLoadForTripAsync(trip, cancellationToken);
        if (dropOffLoad != null)
        {
            await AddLoadAsync(bay, dropOffLoad, cancellationToken);
        }
    }

    public async Task AlertFetchedAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.FetchStarted:
                await SetBayStatusAsync(bay, BayStatus.FetchFinished, cancellationToken);
                break;
            
            case BayStatus.WaitingFetch:
                await SetBayStatusAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            
            default:
                // TODO: Log the miss
                return;
        }
        
        var trip = await GetTripForBayAsync(bay, cancellationToken);
        if (trip == null) return;

        var pickUpLoad = await tripService.GetPickUpLoadForTripAsync(trip, cancellationToken);
        if (pickUpLoad != null)
        {
            await MoveLoadAsync(bay, pickUpLoad, cancellationToken);
        }
    }

    public async Task AlertPickedUpAsync(Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.PickUpStarted)
        { 
            await SetBayStatusAsync(bay, BayStatus.Free, cancellationToken);
            
            var trip = await GetTripForBayAsync(bay, cancellationToken);
            if (trip == null) return;

            var pickUpLoad = await tripService.GetPickUpLoadForTripAsync(trip, cancellationToken);
            if (pickUpLoad != null)
            {
                await RemoveLoadAsync(bay, pickUpLoad, cancellationToken);
            }
            
            await tripService.AlertBayWorkCompleteAsync(trip, cancellationToken);
        }
    }
    
    public async Task AlertBayWorkCompleteAsync(WorkType workType, Bay bay, CancellationToken cancellationToken)
    {
        switch (workType)
        {
            case WorkType.DropOff when
                bay.BayStatus is 
                    not BayStatus.WaitingFetchStart and
                    not BayStatus.WaitingFetch and  // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.PickUpStarted:
                await AlertDroppedOffAsync(bay, cancellationToken);
                break;
            
            case WorkType.Fetch:
                await AlertFetchedAsync(bay, cancellationToken);
                break;
            
            case WorkType.PickUp when
                bay.BayStatus is
                    not BayStatus.Free and          // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.Claimed and
                    not BayStatus.DroppingOffStarted:
                await AlertPickedUpAsync(bay, cancellationToken);
                break;
        }
    }
    
    public async Task AlertFreeAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        // var works = (await GetWorksForBayAsync(bay, cancellationToken))
        //     .AsAsyncEnumerable()
        //     .WithCancellation(cancellationToken);
        //
        // var startedDropOff = false;
        // var startedFetch = false;
        // var startedPickUp = false;
        //
        // await foreach (var work in works)
        // {
        //     if (work.WorkType == work.DropOff)
        //     {
        //         startedDropOff = true;
        //     }
        //     else if (work.WorkType == work.Fetch)
        //     {
        //         startedFetch = true;
        //     }
        //     else if (work.WorkType == work.PickUp)
        //     {
        //         startedPickUp = true;
        //     }
        // }
        // I will assume for now that Bay is not bipolar

        if (bay.BayStatus == BayStatus.Closed)
        {
            await SetBayStatusAsync(bay, BayStatus.Free, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.Free)
        {
            await AlertFreeAsync(bay, cancellationToken);
        }
        
        if (bay.BayStatus == BayStatus.Claimed)
        {
            await SetBayStatusAsync(bay, BayStatus.DroppingOffStarted, cancellationToken);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
            return;
        }

        if (bay.BayStatus is
            BayStatus.DroppingOffStarted or
            BayStatus.WaitingFetchStart)
        {
            await StartFetchAsync(bay, bayStaff, cancellationToken);
            return;
        }

        if (bay.BayStatus is
            BayStatus.FetchStarted or
            BayStatus.FetchFinished)
        {
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.PickUpStarted)
        {
            await StartPickUpAsync(bay, bayStaff, cancellationToken);
        }
    }
}
