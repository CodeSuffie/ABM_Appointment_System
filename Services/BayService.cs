using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services;

public sealed class BayService(
    ILogger<BayService> logger,
    HubRepository hubRepository,
    PelletRepository pelletRepository,
    PelletService pelletService,
    TripService tripService,
    BayRepository bayRepository,
    TripRepository tripRepository,
    WorkRepository workRepository,
    ModelState modelState)
{
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

        var trip = !modelState.ModelConfig.AppointmentSystemMode ?
            await tripService.GetNextAsync(hub, WorkType.Bay, cancellationToken) :
            await tripService.GetNextAsync(hub, bay, cancellationToken);
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
            logger.LogInformation("Bay ({@Bay}) did not have a Trip assigned.",
                bay);
            
            logger.LogDebug("Removing all BayFlags from thisBay ({@Bay}).",
                bay);
            await bayRepository.RemoveAsync(bay, BayFlags.DroppedOff | BayFlags.Fetched | BayFlags.PickedUp, cancellationToken);
            return;
        }

        if (! await pelletService.HasDropOffPelletsAsync(bay, cancellationToken))
        {
            await bayRepository.AddAsync(bay, BayFlags.DroppedOff, cancellationToken);
        }
        else
        {
            await bayRepository.RemoveAsync(bay, BayFlags.DroppedOff, cancellationToken);
        }
        
        if (! await pelletService.HasFetchPelletsAsync(bay, cancellationToken))
        {
            await bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
        }
        else
        {
            await bayRepository.RemoveAsync(bay, BayFlags.Fetched, cancellationToken);
        }
        
        if (! await pelletService.HasPickUpPelletsAsync(bay, cancellationToken))
        {
            await bayRepository.AddAsync(bay, BayFlags.PickedUp, cancellationToken);
        }
        else
        {
            await bayRepository.RemoveAsync(bay, BayFlags.PickedUp, cancellationToken);
        }
    }

    public async Task<bool> HasRoomForPelletAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pelletCount = await pelletRepository.Get(bay)
            .CountAsync(cancellationToken);
        var dropOffWorkCount = await workRepository.Get(bay, WorkType.DropOff)
            .CountAsync(cancellationToken);
        var fetchWorkCount = await workRepository.Get(bay, WorkType.Fetch)
            .CountAsync(cancellationToken);

        return (pelletCount + dropOffWorkCount + fetchWorkCount) < bay.Capacity;
    }
}
