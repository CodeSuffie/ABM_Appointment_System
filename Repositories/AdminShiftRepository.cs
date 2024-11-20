using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AdminShiftRepository(
    ModelDbContext context,
    ModelRepository modelRepository)
{
    public async Task<bool> IsCurrentAsync(AdminShift adminShift, CancellationToken cancellationToken)
    {
        var modelTime = await modelRepository.GetModelTimeAsync(cancellationToken);
        
        if (adminShift.Duration == null)
            throw new Exception("The shift for this AdminStaff does not have a Duration.");
            
        var endTime = (TimeSpan)(adminShift.StartTime + adminShift.Duration);
        
        return modelTime >= adminShift.StartTime && modelTime <= endTime;
    }
    
    public async Task<IQueryable<AdminShift>> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = context.AdminShifts
            .Where(x => x.AdminStaffId == adminStaff.Id);

        return shifts;
    }
    
    public async Task<AdminShift?> GetCurrentAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = (await GetAsync(adminStaff, cancellationToken))
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
}