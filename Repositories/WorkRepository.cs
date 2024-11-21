using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Repositories;

public sealed class WorkRepository(
    ModelDbContext context,
    ModelRepository modelRepository)
{
    public async Task<TimeSpan?> GetTimeAsync(WorkType workType, CancellationToken cancellationToken)
    {
        return workType switch
        {
            WorkType.CheckIn => ModelConfig.CheckInWorkTime,
            WorkType.DropOff => ModelConfig.DropOffWorkTime,
            WorkType.PickUp => ModelConfig.PickUpWorkTime,
            WorkType.Fetch => ModelConfig.FetchWorkTime,
            _ => null
        };
    }
    
    private async Task<Work> GetNewAsync(WorkType workType, CancellationToken cancellationToken)
    {
        var startTime = await modelRepository.GetTimeAsync(cancellationToken);
        var duration = await GetTimeAsync(workType, cancellationToken);
        
        var work = new Work
        {
            StartTime = startTime,
            Duration = duration,
            WorkType = workType,
        };

        return work;
    }
    
    public async Task<Work?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<IQueryable<Work>> GetAsync(Bay bay, WorkType workType, CancellationToken cancellationToken)
    {
        var work = context.Works
            .Where(x => x.BayId == bay.Id && 
                                      x.WorkType == workType);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var work = await (await GetAsync(bay, WorkType.Bay, cancellationToken))
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
    
    public async Task AddAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(workType, cancellationToken);
        work.Trip = trip;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(WorkType.CheckIn, cancellationToken);
        work.Trip = trip;
        work.AdminStaff = adminStaff;
        
        trip.Work = work;
        adminStaff.Work = work;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(WorkType.Bay, cancellationToken);
        work.Trip = trip;
        work.Bay = bay;
        
        trip.Work = work;
        bay.Works.Add(work);

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, BayStaff bayStaff, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(workType, cancellationToken);
        work.BayStaff = bayStaff;
        work.Bay = bay;
        
        bayStaff.Work = work;
        bay.Works.Add(work);

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