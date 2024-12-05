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
    PelletRepository pelletRepository,
    LoadRepository loadRepository,
    TripService tripService,
    BayRepository bayRepository,
    TripRepository tripRepository,
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
    
    public async Task UpdateFlagsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Bay ({@Bay}) did not have a Trip assigned.",
                bay);
            
            await bayRepository.RemoveAsync(bay, BayFlags.DroppedOff | BayFlags.Fetched | BayFlags.PickedUp, cancellationToken);
            return;
        }

        var dropOffLoad = await loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (dropOffLoad == null || !pelletRepository.Get(dropOffLoad).Any())
        {
            if (dropOffLoad == null)
            {
                logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Drop-Off.",
                    trip);
            }
            else
            {
                logger.LogInformation("Load ({@Load}) did not have any more Pellets assigned to Drop-Off.",
                    dropOffLoad);
            }

            await bayRepository.AddAsync(bay, BayFlags.DroppedOff, cancellationToken);
        }
        else
        {
            logger.LogInformation("Load ({@Load}) still has more Pellets assigned to Drop-Off.",
                dropOffLoad);
            
            await bayRepository.RemoveAsync(bay, BayFlags.DroppedOff, cancellationToken);
            return;
        }
        
        var pickUpLoad = await loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                    trip);

            await bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
            await bayRepository.AddAsync(bay, BayFlags.PickedUp, cancellationToken);
        }
        else
        {
            var pickUpPellets = pelletRepository.Get(pickUpLoad);
            if (!pickUpPellets.Any())
            {
                logger.LogInformation("Load ({@Load}) did not have any more Pellets assigned to Pick-Up.",
                    pickUpLoad);

                await bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
                await bayRepository.AddAsync(bay, BayFlags.PickedUp, cancellationToken);
            }
            else
            {
                logger.LogInformation("Load ({@Load}) still has more Pellets assigned to Pick-Up.",
                    dropOffLoad);
                
                await bayRepository.RemoveAsync(bay, BayFlags.PickedUp, cancellationToken);
                
                var fetchPellets = pickUpPellets.Where(p => p.BayId != bay.Id);
                if (!fetchPellets.Any())
                {
                    logger.LogInformation("Load ({@Load}) did not have any more Pellets assigned to Fetch.",
                        pickUpLoad);

                    await bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
                }
                else
                {
                    logger.LogInformation("Load ({@Load}) still has more Pellets assigned to Fetch.",
                        dropOffLoad);
                    
                    await bayRepository.RemoveAsync(bay, BayFlags.Fetched, cancellationToken);
                }
            }
        }
    }
}
