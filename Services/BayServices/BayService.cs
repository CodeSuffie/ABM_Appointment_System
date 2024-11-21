using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.TripServices;
using Settings;

namespace Services.BayServices;

public sealed class BayService(
    HubRepository hubRepository,
    TripService tripService,
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

    public async Task<Bay> SelectBayAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = await (await bayRepository.GetAsync(hub, cancellationToken))
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to this Hub.");

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
        return bay;
    }
    
    public async Task AlertClaimedAsync(Bay bay, Trip trip, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, bay, cancellationToken);
        await bayRepository.SetAsync(bay, BayStatus.Claimed, cancellationToken);
    }
    
    public async Task AlertUnclaimedAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
            throw new Exception("This Bay was just told to be unclaimed but no Trip was assigned");
        
        await tripRepository.UnsetAsync(trip, bay, cancellationToken);
    }
    
    public async Task AlertWorkCompleteAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) 
            throw new Exception("This Bay was just told to have completed the Bay Work but no Trip is assigned");
        
        await tripService.AlertBayWorkCompleteAsync(trip, cancellationToken);
            
        var work = await workRepository.GetAsync(bay, cancellationToken);
        if (work == null) return;
        await workRepository.RemoveAsync(work, cancellationToken);
    }

    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null) 
            throw new Exception("This Bay was just told to be free but no Hub is assigned");

        var newTrip = await tripService.GetNextAsync(hub, WorkType.WaitBay, cancellationToken);
        if (newTrip == null) return;   // TODO: Log no waiting Trips

        await tripService.AlertFreeAsync(newTrip, bay, cancellationToken);
    }
    
    public async Task AlertDroppedOffAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.DroppingOffStarted:
                await bayRepository.SetAsync(bay, BayStatus.WaitingFetchStart, cancellationToken);
                break;
            case BayStatus.FetchStarted:
                await bayRepository.SetAsync(bay, BayStatus.WaitingFetch, cancellationToken);
                break;
            case BayStatus.FetchFinished:
                await bayRepository.SetAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            default:
                // TODO: Log the miss
                return;
        }

        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) return;

        var dropOffLoad = await loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (dropOffLoad != null)
        {
            await loadRepository.SetAsync(dropOffLoad, bay, cancellationToken);
        }
    }

    public async Task AlertFetchedAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.FetchStarted:
                await bayRepository.SetAsync(bay, BayStatus.FetchFinished, cancellationToken);
                break;
            
            case BayStatus.WaitingFetch:
                await bayRepository.SetAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                break;
            
            default:
                // TODO: Log the miss
                return;
        }
        
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) return;

        var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad != null)
        {
            await loadRepository.SetAsync(pickUpLoad, bay, cancellationToken);
        }
    }
    
    public async Task AlertPickedUpAsync(Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus is BayStatus.PickUpStarted or BayStatus.WaitingFetch)
        { 
            await bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
            
            var trip = await tripRepository.GetAsync(bay, cancellationToken);
            if (trip == null) return;
            
            var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
            if (pickUpLoad != null)
            {
                if (bay.BayStatus is BayStatus.PickUpStarted)
                {
                    await loadRepository.UnsetAsync(pickUpLoad, bay, cancellationToken);
                }
                else
                {
                    // TODO: Log Load miss
                    await loadRepository.UnsetPickUpAsync(pickUpLoad, trip, cancellationToken);
                }
            }
            
            await AlertWorkCompleteAsync(bay, cancellationToken);
        }
    }
}
