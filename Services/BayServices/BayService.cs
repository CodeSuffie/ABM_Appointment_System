using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.TripServices;
using Settings;

namespace Services.BayServices;

public sealed class BayService(
    HubRepository hubRepository,
    TripService tripService,
    WorkService workService,
    BayRepository bayRepository,
    TripRepository tripRepository,
    LoadRepository loadRepository,
    WorkRepository workRepository)
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

    public async Task<Bay> SelectBayByHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = await (await bayRepository.GetBaysByHubAsync(hub, cancellationToken))
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to this Hub.");

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
        return bay;
    }

    public async Task StartDropOffAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workRepository.AddWorkAsync(bay, bayStaff, WorkType.DropOff, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetTripByBayAsync(bay, cancellationToken);
        if (trip == null)
            throw new Exception("Cannot start a Fetch Work Job for a BayStaff at a Bay that has no Trip assigned.");
        
        var pickUpLoad = await loadRepository.GetPickUpLoadByTripAsync(trip, cancellationToken);
        
        if (pickUpLoad != null)
        {
            var pickUpLoadBay = await bayRepository.GetBayByLoadAsync(pickUpLoad, cancellationToken);
            
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
                await workRepository.AddWorkAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
                return;
            }
        }
        
        // TODO: Not fetched but done
        await AlertFetchedAsync(bay, cancellationToken);
        await AlertFreeAsync(bay, bayStaff, cancellationToken);
    }
    
    public async Task StartPickUpAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workRepository.AddWorkAsync(bay, bayStaff, WorkType.PickUp, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    
    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        // TODO: AlertDone to Trip?
        var oldTrip = await tripRepository.GetTripByBayAsync(bay, cancellationToken);
        if (oldTrip != null)
        {
            await tripRepository.RemoveTripBayAsync(oldTrip, bay, cancellationToken);
        }

        var hub = await hubRepository.GetHubByBayAsync(bay, cancellationToken);
        if (hub == null) return;
        
        var newTrip = await tripRepository.GetNextTripByHubByWorkTypeAsync(hub, WorkType.WaitBay, cancellationToken);
        if (newTrip != null)
        {
            await tripService.AlertFreeAsync(newTrip, bay, cancellationToken);
            await bayRepository.SetBayStatusAsync(bay, BayStatus.Claimed, cancellationToken);
        }
    }

    public async Task AlertDroppedOffAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.DroppingOffStarted:
                await bayRepository.SetBayStatusAsync(bay, BayStatus.WaitingFetchStart, cancellationToken);
                break;
            case BayStatus.FetchStarted:
                await bayRepository.SetBayStatusAsync(bay, BayStatus.WaitingFetch, cancellationToken);
                break;
            case BayStatus.FetchFinished:
                await bayRepository.SetBayStatusAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            default:
                // TODO: Log the miss
                return;
        }

        var trip = await tripRepository.GetTripByBayAsync(bay, cancellationToken);
        if (trip == null) return;

        var dropOffLoad = await loadRepository.GetDropOffLoadByTripAsync(trip, cancellationToken);
        if (dropOffLoad != null)
        {
            await loadRepository.SetLoadBayAsync(dropOffLoad, bay, cancellationToken);
        }
    }

    public async Task AlertFetchedAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.FetchStarted:
                await bayRepository.SetBayStatusAsync(bay, BayStatus.FetchFinished, cancellationToken);
                break;
            
            case BayStatus.WaitingFetch:
                await bayRepository.SetBayStatusAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            
            default:
                // TODO: Log the miss
                return;
        }
        
        var trip = await tripRepository.GetTripByBayAsync(bay, cancellationToken);
        if (trip == null) return;

        var pickUpLoad = await loadRepository.GetPickUpLoadByTripAsync(trip, cancellationToken);
        if (pickUpLoad != null)
        {
            await loadRepository.SetLoadBayAsync(pickUpLoad, bay, cancellationToken);
        }
    }

    public async Task AlertPickedUpAsync(Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.PickUpStarted)
        { 
            await bayRepository.SetBayStatusAsync(bay, BayStatus.Free, cancellationToken);
            
            var trip = await tripRepository.GetTripByBayAsync(bay, cancellationToken);
            if (trip == null) return;

            var pickUpLoad = await loadRepository.GetPickUpLoadByTripAsync(trip, cancellationToken);
            if (pickUpLoad != null)
            {
                await loadRepository.RemoveLoadBayAsync(pickUpLoad, bay, cancellationToken);
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
            await bayRepository.SetBayStatusAsync(bay, BayStatus.Free, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.Free)
        {
            await AlertFreeAsync(bay, cancellationToken);
        }
        
        if (bay.BayStatus == BayStatus.Claimed)
        {
            await bayRepository.SetBayStatusAsync(bay, BayStatus.DroppingOffStarted, cancellationToken);
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
