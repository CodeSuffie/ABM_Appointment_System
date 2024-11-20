using Database;
using Database.Models;

namespace Repositories;

public sealed class BayStaffRepository(ModelDbContext context)
{
    public async Task AddAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await context.BayStaffs
            .AddAsync(bayStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}