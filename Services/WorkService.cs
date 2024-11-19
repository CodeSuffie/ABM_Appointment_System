using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class WorkService(
    ModelDbContext context,
    ModelService modelService)
{
    public async Task<Work> GetNewObjectAsync(WorkType workType, CancellationToken cancellationToken)
    {
        var startTime = await modelService.GetModelTimeAsync(cancellationToken);
        var duration = await GetWorkTimeAsync(workType, cancellationToken);
        
        var work = new Work
        {
            StartTime = startTime,
            Duration = duration,
            WorkType = workType,
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
    
    public async Task<Bay?> GetBayForWorkAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.BayId == null) return null;
        
        var bay = await context.Bays
            .FirstOrDefaultAsync(x => x.Id == work.BayId, cancellationToken);

        return bay;
    }

    public async Task<Trip?> GetTripForWorkAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.TripId == null) return null;
        
        var trip = await context.Trips
            .FirstOrDefaultAsync(x => x.Id == work.TripId, cancellationToken);

        return trip;
    }
    
    public async Task AddWorkAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(workType, cancellationToken);
        work.Trip = trip;

        // TODO: Repository
        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddWorkAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(WorkType.CheckIn, cancellationToken);
        work.Trip = trip;
        work.AdminStaff = adminStaff;

        // TODO: Repository
        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddWorkAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(WorkType.Bay, cancellationToken);
        work.Trip = trip;
        work.Bay = bay;

        // TODO: Repository
        await context.Works
            .AddAsync(work, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddWorkAsync(Bay bay, BayStaff bayStaff, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(workType, cancellationToken);
        work.Bay = bay;
        work.BayStaff = bayStaff;

        // TODO: Repository
        await context.Works
            .AddAsync(work, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    // TODO: Repository
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

    public async Task AdaptWorkLoadAsync(Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Adapt the workload at the bay to include new changes such as additional manpower
    }
}