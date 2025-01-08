using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

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
    
    public Task<BayShift?> GetCurrentAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        return bayShiftRepository.Get(bayStaff)
            .FirstOrDefaultAsync(bs => bs.StartTime <= modelState.ModelTime && 
                                       bs.StartTime + bs.Duration >= modelState.ModelTime,
                cancellationToken);
    }
    
    public IQueryable<BayShift> GetCurrent(Bay bay, CancellationToken cancellationToken)
    {
        return bayShiftRepository.Get(bay)
            .Where(bs => bs.StartTime <= modelState.ModelTime &&
                         bs.StartTime + bs.Duration >= modelState.ModelTime);
    }
}