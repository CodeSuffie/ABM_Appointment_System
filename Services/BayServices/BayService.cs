using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;
using Services.TripServices;

namespace Services.BayServices;

public sealed class BayService(
    ILogger<BayService> logger,
    HubRepository hubRepository,
    TripService tripService,
    BayRepository bayRepository,
    TripRepository tripRepository,
    LoadRepository loadRepository,
    WorkRepository workRepository,
    LocationService locationService,
    ModelState modelState)
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

        logger.LogDebug("Setting Bay \n({@Bay})\n to its Hub \n({@Hub})",
            bay,
            hub);
        await bayRepository.SetAsync(bay, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Bay \n({@Bay})",
            bay);
        await locationService.InitializeObjectAsync(bay, cancellationToken);

        return bay;
    }

    public async Task<Bay?> SelectBayAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = await (bayRepository.Get(hub))
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0)
        {
            logger.LogError("Hub \n({@Hub})\n did not have a Bay assigned.",
                hub);

            return null;
        }

        var bay = bays[modelState.Random(bays.Count)];
        return bay;
    }
    
    public async Task AlertWorkCompleteAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) 
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to alert completed Work for",
                bay);

            return;
        }
        
        logger.LogDebug("Alerting Bay Work Completed for this Bay \n({@Bay})\n to assigned Trip \n({@Trip})",
            bay,
            trip);
        await tripService.AlertBayWorkCompleteAsync(trip, cancellationToken);
            
        var work = await workRepository.GetAsync(bay, cancellationToken);
        if (work == null) return;
        
        logger.LogDebug("Removing completed Work \n({@Work})\n for this Bay \n({@Bay})",
            work,
            bay);
        await workRepository.RemoveAsync(work, cancellationToken);
    }

    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Hub assigned to alert free for.",
                bay);

            return;
        }

        var trip = await tripService.GetNextAsync(hub, WorkType.WaitBay, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Hub \n({@Hub})\n did not have a Trip for this Bay \n({@Bay})\n to assign Bay Work for.",
                hub,
                bay);
            
            logger.LogDebug("Bay \n({@Bay})\n will remain idle...",
                bay);
            
            return;
        }

        logger.LogDebug("Alerting Free for this Bay \n({@Bay})\n to selected Trip \n({@Trip})",
            bay,
            trip);
        await tripService.AlertFreeAsync(trip, bay, cancellationToken);
    }
    
    public async Task AlertDroppedOffAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.DroppingOffStarted:
                
                logger.LogInformation("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                    bay,
                    BayStatus.DroppingOffStarted,
                    BayStatus.WaitingFetchStart);
                
                logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.WaitingFetchStart,
                    bay);
                await bayRepository.SetAsync(bay, BayStatus.WaitingFetchStart, cancellationToken);
                
                break;
            
            case BayStatus.FetchStarted:
                
                logger.LogInformation("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                    bay,
                    BayStatus.FetchStarted,
                    BayStatus.WaitingFetch);
                
                logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.WaitingFetch,
                    bay);
                await bayRepository.SetAsync(bay, BayStatus.WaitingFetch, cancellationToken);
                
                break;
            
            case BayStatus.FetchFinished:
                
                logger.LogInformation("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                    bay,
                    BayStatus.FetchFinished,
                    BayStatus.PickUpStarted);
                
                logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.PickUpStarted,
                    bay);
                await bayRepository.SetAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                
                break;
            
            default:
                
                logger.LogError("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} does not have a BayStatus to alert Dropped Off for.",
                    bay,
                    bay.BayStatus);
                
                return;
        }

        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to get the Load to Drop Off for.",
                bay);

            return;
        }

        var dropOffLoad = await loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (dropOffLoad == null)
        {
            logger.LogInformation("Trip \n({@Trip})\n at Bay \n({@Bay})\n did not have a Load assigned to Drop Off.",
                trip,
                bay);
            
            return;
        }
        
        // TODO: BIG REFACTOR HERE
        logger.LogDebug("Moving the Dropped Off Load \n({@Load})\n for this Trip \n({@Trip})\n to this Bay \n({@Bay})",
            dropOffLoad,
            trip,
            bay);
        // await loadRepository.SetAsync(dropOffLoad, bay, cancellationToken);
        
        logger.LogDebug("Setting Dropped Off Load \n({@Load})\n for this Trip \n({@Trip})\n to be of type {LoadType}...",
            dropOffLoad,
            trip,
            LoadType.PickUp);
        await loadRepository.SetAsync(dropOffLoad, LoadType.PickUp, cancellationToken);
    }

    public async Task AlertFetchedAsync(Bay bay, CancellationToken cancellationToken)
    {
        switch (bay.BayStatus)
        {
            case BayStatus.DroppingOffStarted:
            case BayStatus.FetchStarted:
                
                logger.LogInformation("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                    bay,
                    BayStatus.FetchStarted,
                    BayStatus.FetchFinished);
                
                logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.FetchFinished,
                    bay);
                await bayRepository.SetAsync(bay, BayStatus.FetchFinished, cancellationToken);
                
                break;
            
            case BayStatus.WaitingFetch:
            case BayStatus.WaitingFetchStart:
                
                logger.LogInformation("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                    bay,
                    bay.BayStatus,
                    BayStatus.PickUpStarted);
                
                logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.PickUpStarted,
                    bay);
                await bayRepository.SetAsync(bay, BayStatus.PickUpStarted, cancellationToken);
                
                break;
            
            default:
                logger.LogError("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} does not have a BayStatus to alert Fetched for.",
                    bay,
                    bay.BayStatus);
                
                return;
        }
        
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to get the Load to PickUp for.",
                bay);

            return;
        }

        var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            logger.LogInformation("Trip \n({@Trip})\n at Bay \n({@Bay})\n did not have a Load assigned to Pick Up.",
                trip,
                bay);

            return;
        }
        
        // TODO: BIG REFACTOR HERE
        logger.LogDebug("Moving the Fetched Load \n({@Load})\n for this Trip \n({@Trip})\n to this Bay \n({@Bay})",
            pickUpLoad,
            trip,
            bay);
        // await loadRepository.SetAsync(pickUpLoad, bay, cancellationToken);
    }
    
    public async Task AlertPickedUpAsync(Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.PickUpStarted)
        {
            logger.LogInformation(
                "Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} can set its BayStatus to {@BayStatus}.",
                bay,
                BayStatus.PickUpStarted,
                BayStatus.Free);

            logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                BayStatus.Free,
                bay);
            await bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
        }
        else
        {
            logger.LogError("Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} does not have a BayStatus to alert Picked Up for.",
                bay,
                bay.BayStatus);
                
            return;
        }

        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to get the Load to PickUp for.",
                bay);

            return;
        }
        
        var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            logger.LogInformation("Trip \n({@Trip})\n at Bay \n({@Bay})\n did not have a Load assigned to Pick Up.",
                trip,
                bay);
        }
        else
        {
            // TODO: BIG REFACTOR HERE
            logger.LogDebug("Removing the Picked Up Load \n({@Load})\n for this Trip \n({@Trip})\n from this Bay \n({@Bay})",
                        pickUpLoad,
                        trip,
                        bay);
            // await loadRepository.UnsetAsync(pickUpLoad, bay, cancellationToken);
        }
        
        
        logger.LogDebug("Alerting Work completed for this Bay \n({@Bay})",
            bay);
        await AlertWorkCompleteAsync(bay, cancellationToken);
    }
}
