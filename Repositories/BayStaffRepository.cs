using Database;
using Database.Models;

namespace Repositories;

public sealed class BayStaffRepository(ModelDbContext context)
{
    public IQueryable<BayStaff> Get()
    {
        var bayStaffs = context.BayStaffs;

        return bayStaffs;
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
}