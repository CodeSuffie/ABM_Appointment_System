using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AdminStaffRepository(ModelDbContext context)
{
    public IQueryable<AdminStaff> Get()
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

    public async Task<AdminStaff?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        var adminStaff = await context.AdminStaffs
            .FirstOrDefaultAsync(a => a.Id == work.AdminStaffId, cancellationToken);

        return adminStaff;
    }
    
    public async Task AddAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await context.AdminStaffs
            .AddAsync(adminStaff, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(AdminStaff adminStaff, AdminShift adminShift, CancellationToken cancellationToken)
    {
        adminStaff.Shifts.Add(adminShift);
        adminShift.AdminStaff = adminStaff;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}