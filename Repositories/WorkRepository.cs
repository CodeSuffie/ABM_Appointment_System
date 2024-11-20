using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Repositories;

public sealed class WorkRepository(
    ModelDbContext context,
    ModelRepository modelRepository)
{
    public async Task<TimeSpan?> GetWorkTimeAsync(WorkType workType, CancellationToken cancellationToken)
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
    
    private async Task<Work> GetNewObjectAsync(WorkType workType, CancellationToken cancellationToken)
    {
        var startTime = await modelRepository.GetModelTimeAsync(cancellationToken);
        var duration = await GetWorkTimeAsync(workType, cancellationToken);
        
        var work = new Work
        {
            StartTime = startTime,
            Duration = duration,
            WorkType = workType,
        };

        return work;
    }
    
    public async Task<Work?> GetWorkByTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetWorkByStaffAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.AdminStaff != null &&
                                      x.AdminStaffId == adminStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetWorkByStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.BayStaff != null &&
                                      x.BayStaffId == bayStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task AddWorkAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(workType, cancellationToken);
        work.Trip = trip;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddWorkAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(WorkType.CheckIn, cancellationToken);
        work.Trip = trip;
        work.AdminStaff = adminStaff;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddWorkAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(WorkType.Bay, cancellationToken);
        work.Trip = trip;
        work.Bay = bay;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddWorkAsync(Bay bay, BayStaff bayStaff, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(workType, cancellationToken);
        work.Bay = bay;
        work.BayStaff = bayStaff;

        await context.Works
            .AddAsync(work, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RemoveWorkAsync(Work work, CancellationToken cancellationToken)
    {
        context.Works
            .Remove(work);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}