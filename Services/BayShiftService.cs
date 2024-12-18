using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services;

public sealed class BayShiftService(
    ILogger<BayShiftService> logger,
    BayShiftRepository bayShiftRepository,
    ModelState modelState)
{
    private bool IsCurrent(BayShift bayShift)
    {
        var endTime = bayShift.StartTime + bayShift.Duration;
        
        return modelState.ModelTime >= bayShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<BayShift?> GetCurrentAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = bayShiftRepository.Get(bayStaff)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("BayShift \n({@BayShift})\n is currently active.",
                shift);
                
            return shift;
        }

        logger.LogInformation("No BayShift is currently active.");
        return null;
    }
}