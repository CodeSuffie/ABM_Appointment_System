using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class BayShiftRepository(
    ModelDbContext context,
    ModelRepository modelRepository)
{
    
    public async Task<bool> IsCurrentAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var modelTime = await modelRepository.GetModelTimeAsync(cancellationToken);
        
        if (bayShift.Duration == null)
            throw new Exception("The shift for this BayStaff does not have a Duration.");
            
        var endTime = (TimeSpan)(bayShift.StartTime + bayShift.Duration);
        
        return modelTime >= bayShift.StartTime && modelTime <= endTime;
    }
    
    public async Task<IQueryable<BayShift>> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayStaffId == bayStaff.Id);

        return shifts;
    }
    
    public async Task<BayShift?> GetCurrentAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = (await GetAsync(bayStaff, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (await IsCurrentAsync(shift, cancellationToken))
            {
                return shift;
            }
        }

        return null;
    }
    
    public async Task<IQueryable<BayShift>> GetShiftsByBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayId == bay.Id);

        return shifts;
    }
    
    public async Task<List<BayShift>> GetCurrentShiftsByBayAsync(Bay bay, CancellationToken cancellationToken)
    { 
        var shifts = (await GetShiftsByBayAsync(bay, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
    
        var currentShifts = new List<BayShift>();
    
        await foreach (var shift in shifts)
        {
            if (await IsCurrentAsync(shift, cancellationToken))
            {
                currentShifts.Add(shift);
            }
        }
        
        return currentShifts;
    }
}