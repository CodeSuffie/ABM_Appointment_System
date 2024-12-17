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

    public async Task AddAsync(AdminStaff adminStaff, HubShift hubShift, CancellationToken cancellationToken)
    {
        adminStaff.Shifts.Remove(hubShift);
        adminStaff.Shifts.Add(hubShift);
        hubShift.AdminStaff = adminStaff;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(TimeSpan time, CancellationToken cancellationToken)
    {
        return Get()
            .Where(ads => ads.Shifts
                .Any(sh => sh.StartTime <= time && 
                           sh.StartTime + sh.Duration >= time))
            .CountAsync(cancellationToken);
    }

    public Task<int> CountOccupiedAsync(CancellationToken cancellationToken)
    {
        return Get()
            .Where(ads => ads.Work != null)
            .CountAsync(cancellationToken);
    }
}