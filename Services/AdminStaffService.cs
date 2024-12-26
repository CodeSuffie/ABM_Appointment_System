using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services;

public sealed class AdminStaffService(
    ILogger<AdminStaffService> logger,
    HubRepository hubRepository,
    TripRepository tripRepository,
    TripService tripService,
    WorkRepository workRepository,
    ModelState modelState)
{
    public async Task AlertWorkCompleteAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(adminStaff, cancellationToken);
        if (trip == null)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Trip assigned to alert completed Work for.", adminStaff);

            return;
        }
        
        logger.LogDebug("Alerting Check-In Completed for this AdminStaff \n({@AdminStaff})\n to assigned Trip \n({@Trip})", adminStaff, trip);
        await tripService.AlertCheckInCompleteAsync(trip, cancellationToken);
        
        var work = await workRepository.GetAsync(adminStaff, cancellationToken);
        if (work == null) return;
        
        logger.LogDebug("Removing completed Work \n({@Work})\n for this AdminStaff \n({@AdminStaff})", work, adminStaff);
        await workRepository.RemoveAsync(work, cancellationToken);
    }
    
    public async Task AlertFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken) 
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to alert free for.", adminStaff);

            return;
        }
        
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitCheckIn, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Hub \n({@Hub})\n did not have a Trip for this AdminStaff \n({@AdminStaff})\n to assign Check-In Work for.", hub, adminStaff);
            
            logger.LogDebug("AdminStaff \n({@AdminStaff})\n will remain idle...", adminStaff);
            
            return;
        }

        logger.LogDebug("Alerting Free for this AdminStaff \n({@AdminStaff})\n to selected Trip \n({@Trip})", adminStaff, trip);
        await tripService.AlertFreeAsync(trip, adminStaff, cancellationToken);
    }
}
