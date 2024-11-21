using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AdminStaffRepository(ModelDbContext context)
{
    public async Task<IQueryable<AdminStaff>> GetAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = context.AdminStaffs;

        return adminStaffs;
    }
    
    public async Task<AdminStaff?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var adminStaff = await context.AdminStaffs
            .FirstOrDefaultAsync(a => a.TripId == trip.Id, cancellationToken);

        return adminStaff;
    }
    
    public async Task AddAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await context.AdminStaffs
            .AddAsync(adminStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}