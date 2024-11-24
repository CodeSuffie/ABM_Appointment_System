using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Repositories;

public sealed class WorkRepository(
    ModelDbContext context)
{
    public async Task<Work?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    public IQueryable<Work> Get(Bay bay, WorkType workType)
    {
        var work = context.Works
            .Where(x => x.BayId == bay.Id && 
                                      x.WorkType == workType);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var work = await Get(bay, WorkType.Bay)
            .FirstOrDefaultAsync(cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.AdminStaff != null &&
                                      x.AdminStaffId == adminStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.BayStaff != null &&
                                      x.BayStaffId == bayStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task AddAsync(Work work, CancellationToken cancellationToken)
    {
        await context.Works
            .AddAsync(work, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RemoveAsync(Work work, CancellationToken cancellationToken)
    {
        context.Works
            .Remove(work);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDurationAsync(Work work, TimeSpan duration, CancellationToken cancellationToken)
    {
        work.Duration = duration;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}