using Database;
using Database.Models;

namespace Repositories;

public sealed class AdminStaffRepository(ModelDbContext context)
{
    public async Task AddAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await context.AdminStaffs
            .AddAsync(adminStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}