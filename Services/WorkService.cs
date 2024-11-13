using Database;
using Database.Models;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class WorkService(
    ModelDbContext context,
    ModelService modelService
    )
{
    public async Task<Work> GetNewObjectAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var startTime = await modelService.GetModelTimeAsync(cancellationToken);
        var duration = await GetWorkTimeAsync(workType, cancellationToken);
        
        var work = new Work
        {
            StartTime = startTime,
            Duration = duration,
            WorkType = workType,
            Trip = trip
        };

        return work;
    }

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
    
    public async Task AddWorkAsync(AdminStaff adminStaff, Trip trip, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(trip, WorkType.CheckIn, cancellationToken);
        
        work.AdminStaff = adminStaff;

        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddWorkAsync(BayStaff bayStaff, Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(trip, workType, cancellationToken);
        
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
    
    public async Task<bool> IsWorkCompletedAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.Duration == null) return false;
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        var modelTime = await modelService.GetModelTimeAsync(cancellationToken);
        
        return endTime <= modelTime;
    }

    
}