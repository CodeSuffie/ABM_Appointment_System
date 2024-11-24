using Database;
using Database.Models;
using Database.Models.Logging;

namespace Repositories;

public sealed class BayStaffRepository(ModelDbContext context)
{
    public Task<IQueryable<BayStaff>> GetAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = context.BayStaffs;

        return Task.FromResult<IQueryable<BayStaff>>(bayStaffs);
    }
    
    public async Task AddAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await context.BayStaffs
            .AddAsync(bayStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(BayStaff bayStaff, BayStaffLog log, CancellationToken cancellationToken)
    {
        bayStaff.BayStaffLogs.Add(log);
        log.BayStaff = bayStaff;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}