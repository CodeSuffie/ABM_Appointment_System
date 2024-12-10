using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class BayStaffRepository(ModelDbContext context)
{
    public IQueryable<BayStaff> Get()
    {
        var bayStaffs = context.BayStaffs;

        return bayStaffs;
    }

    public async Task<BayStaff?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        var bayStaff = await Get()
            .FirstOrDefaultAsync(bs => bs.Id == work.BayStaffId, cancellationToken);

        return bayStaff;
    }
    
    public async Task AddAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await context.BayStaffs
            .AddAsync(bayStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(BayStaff bayStaff, BayShift bayShift, CancellationToken cancellationToken)
    {
        bayStaff.Shifts.Add(bayShift);
        bayShift.BayStaff = bayStaff;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(TimeSpan time, CancellationToken cancellationToken)
    {
        return Get()
            .Where(bs => bs.Shifts
                .Any(sh => sh.StartTime <= time && 
                                             sh.StartTime + sh.Duration >= time))
            .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(WorkType workType, CancellationToken cancellationToken)
    {
        return Get()
            .Where(bs => bs.Work != null &&
                        bs.Work.WorkType == workType)
            .CountAsync(cancellationToken);
    }
}