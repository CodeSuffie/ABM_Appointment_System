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
            Opened = false,
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
    public async Task<IQueryable<BayShift>> GetShiftsForBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayId == bay.Id);

        return shifts;
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
    public async Task<IQueryable<Work>> GetShiftsForAdminStaffAsync(Bay bay, CancellationToken cancellationToken)
    {
        var works = context.Works
            .Where(x => x.Bay != null && x.BayId == bay.Id);

        return works;
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
        await workService.ChangeWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Trip trip, Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var pickUpLoad = tripService.GetPickUpLoadForTripAsync(trip, cancellationToken);
        if (pickUpLoad != null)
        {
            var pickUpLoadBay = loadService.GetBayForLoadAsync(pickUpLoad, cancellationToken);
            if (pickUpLoadBay != null)
            {
                if (pickUpLoadBay.Id != bay.Id)
                {
                    await workService.AddWorkAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
                    return;
                }

                tripService.AlertFetchedAsync(trip, cancellationToken);
            }
            else
            {
                // TODO: Log that Load has not arrived yet
            }
        }
    }
    
    public async Task StartPickUpAsync(CancellationToken cancellationToken)
    {
        
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
            // TODO: Start AdminStaff Work
        }
    }

    public async Task AlertFreeAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await GetTripForBayAsync(bay, cancellationToken);
        if (trip == null)
            throw new Exception("The Trip assigned to this Bay has been removed but its BayStaff has just completed" +
                                "Work for that Trip and AlertFree has not been called by the Bay");

        var works = (await GetWorksForBayAsync(bay, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var startedDropOff = false;
        var startedFetch = false;
        var startedPickUp = false;
        
        await foreach (var work in works)
        {
            if (work.WorkType == work.DropOff)
            {
                startedDropOff = true;
            }
            else if (work.WorkType == work.Fetch)
            {
                startedFetch = true;
            }
            else if (work.WorkType == work.PickUp)
            {
                startedPickUp = true;
            }
        }

        if (startedDropOff && startedPickUp ||
            startedDropOff && trip.DroppedOff ||
            startedDropOff && trip.PickedUp ||
            startedPickUp && trip.PickedUp)
            throw new Exception("DropOff or PickUp Work are not being executed in the correct order.");
        
        if (trip is { DroppedOff: false, PickedUp: false } && !startedPickUp)
        {
            if (startedDropOff && !startedFetch && trip is { Fetched: false })
            {
                await StartFetchAsync(trip, bay, bayStaff, cancellationToken);
            }
            else
            {
                await StartDropOffAsync(bay, bayStaff, cancellationToken);
            }
        }
        else if (trip is {DroppedOff: true, PickedUp: false})
        {
            
        }
        
        // TODO: If Truck at my Bay, If PickUp Load is not available at this Bay, and no one else is fetching it, fetch the Load
        // TODO: If Truck at my Bay, continue handling their Trip
    }

    public async Task AlertShiftEndAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = await bayShiftService.GetCurrentShiftsAsync(bay, cancellationToken);
        if (shifts.Count > 1) return;
        
        bay.Opened = false;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AlertShiftStartAsync(Bay bay, CancellationToken cancellationToken)
    {
        bay.Opened = true;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}
