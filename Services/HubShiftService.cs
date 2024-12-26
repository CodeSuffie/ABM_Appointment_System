using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services;

public sealed class HubShiftService(
    ILogger<HubShiftService> logger,
    HubShiftRepository hubShiftRepository,
    ModelState modelState) 
{
    private bool IsCurrent(HubShift hubShift)
    {
        var endTime = hubShift.StartTime + hubShift.Duration;
        
        return modelState.ModelTime >= hubShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<HubShift?> GetCurrentAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(adminStaff)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this AdminStaff \n({@AdminStaff})\n.", shift, adminStaff);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this AdminStaff \n({@AdminStaff})\n.", adminStaff);
        return null;
    }

    public async Task<HubShift?> GetCurrentAsync(Picker picker, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(picker)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this Picker \n({@Picker})\n.", shift, picker);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this Picker \n({@Picker})\n.", picker);
        return null;
    }
    
    public async Task<HubShift?> GetCurrentAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(stuffer)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this Stuffer \n({@Stuffer})\n.", shift, stuffer);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this Stuffer \n({@Stuffer})\n.", stuffer);
        return null;
    }
}