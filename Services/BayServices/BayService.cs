using System.Diagnostics.Eventing.Reader;
using Database.Models;
using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.BayStaffServices;
using Services.HubServices;
using Services.ModelServices;
using Services.TripServices;
using Settings;

namespace Services.BayServices;

public sealed class BayService(
    HubRepository hubRepository,
    TripService tripService,
    BayRepository bayRepository,
    TripRepository tripRepository,
    LoadRepository loadRepository,
    WorkRepository workRepository,
    TripLogger tripLogger,
    BayLogger bayLogger,
    HubLogger hubLogger,
    ModelState modelState)
{
    public Task<Bay> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            XSize = 1,
            YSize = 1,
            BayStatus = BayStatus.Closed,
            Hub = hub,
        };

        return Task.FromResult(bay);
    }

    public async Task<Bay> SelectBayAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = await (await bayRepository.GetAsync(hub, cancellationToken))
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to this Hub.");

        var bay = bays[modelState.Random(bays.Count)];
        return bay;
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
        if (newTrip == null)
        {
            await hubLogger.LogAsync(hub, bay, LogType.Info, "No Trips waiting for a Bay.", cancellationToken);
            return;
        }

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
                await bayLogger.LogAsync(
                    bay,
                    bay.BayStatus,
                    LogType.Warning,
                    "Not a Status to Alert Dropped Off for.",
                    cancellationToken);
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
                await bayLogger.LogAsync(
                    bay,
                    bay.BayStatus,
                    LogType.Warning,
                    "Not a Status to Alert Fetched for.",
                    cancellationToken);
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
                    await tripLogger.LogAsync(trip, pickUpLoad, LogType.Error, "Not arrived, failed to Pick Up", cancellationToken);
                    await bayLogger.LogAsync(bay, pickUpLoad, LogType.Error, "Not arrived, failed to Pick Up.", cancellationToken);
                    
                    await loadRepository.UnsetPickUpAsync(pickUpLoad, trip, cancellationToken);
                }
            }
            
            await AlertWorkCompleteAsync(bay, cancellationToken);
        }
    }
}
