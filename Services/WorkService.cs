using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class WorkService(
    ILogger<WorkService> logger,
    WorkRepository workRepository,
    ModelState modelState)
{
    public bool IsWorkCompleted(Work work)
    {
        if (work.Duration == null)
        {
            logger.LogError("Work ({@Work}) does not have a Duration",
                work);

            return false;
        }
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        
        return endTime <= modelState.ModelTime;
    }
    
    public async Task AdaptWorkLoadAsync(Bay bay, CancellationToken cancellationToken)
    {
        WorkType? workType = bay.BayStatus switch
        {
            BayStatus.DroppingOffStarted or BayStatus.FetchStarted or BayStatus.FetchFinished => WorkType.DropOff,
            BayStatus.PickUpStarted => WorkType.PickUp,
            _ => null
        };

        if (workType == null)
        {
            logger.LogError("Bay ({@Bay}) with BayStatus {@BayStatus} does not have a valid BayStatus to adapt the workload for.",
                bay,
                bay.BayStatus);
            
            return;
        }
        
        logger.LogDebug("Adapting workload for Work at this Bay ({@Bay}) with WorkType {@WorkType}...",
            bay,
            workType);

        var works = workRepository.Get(bay, (WorkType) workType)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

        var totalDuration = new TimeSpan(0, 0, 0);
        var count = 0;
        await foreach (var work in works)
        {
            if (work.Duration != null)
            {
                totalDuration += (TimeSpan) work.Duration;
            }
            count += 1;
        }
        
        var newDuration = (totalDuration / count);
        
        logger.LogInformation("Total Duration for Work at this Bay ({@Bay}) with WorkType {@WorkType} is " +
                              "{TimeSpan} spread over {Count} works. The new duration will be set to {TimeSpan}",
            bay,
            workType,
            totalDuration,
            count,
            newDuration);

        await foreach (var work in works)
        {
            logger.LogDebug("Setting Duration of this Work ({@Work}) for this Bay ({@Bay}) to {TimeSpan}...",
                work,
                bay,
                newDuration);
            await workRepository.SetDurationAsync(work, (totalDuration / count), cancellationToken);
        }
    }
    
    public TimeSpan? GetTime(WorkType workType)
    {
        return workType switch
        {
            WorkType.CheckIn => modelState.ModelConfig.CheckInWorkTime,
            WorkType.DropOff => modelState.ModelConfig.DropOffWorkTime,
            WorkType.PickUp => modelState.ModelConfig.PickUpWorkTime,
            WorkType.Fetch => modelState.ModelConfig.FetchWorkTime,
            _ => null
        };
    }
    
    private Work GetNew(WorkType workType)
    {
        var duration = GetTime(workType);
        
        var work = new Work
        {
            StartTime = modelState.ModelTime,
            Duration = duration,
            WorkType = workType,
        };

        return work;
    }
    
    public async Task AddAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = GetNew(workType);
        work.Trip = trip;

        await workRepository.AddAsync(work, cancellationToken);
    }
    
    public async Task AddAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = GetNew(WorkType.CheckIn);
        work.Trip = trip;
        work.AdminStaff = adminStaff;
        
        trip.Work = work;
        adminStaff.Work = work;

        await workRepository.AddAsync(work, cancellationToken);
    }

    public async Task AddAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = GetNew(WorkType.Bay);
        work.Trip = trip;
        work.Bay = bay;
        
        trip.Work = work;
        bay.Works.Add(work);

        await workRepository.AddAsync(work, cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, BayStaff bayStaff, WorkType workType, CancellationToken cancellationToken)
    {
        var work = GetNew(workType);
        work.BayStaff = bayStaff;
        work.Bay = bay;
        
        bayStaff.Work = work;
        bay.Works.Add(work);

        await workRepository.AddAsync(work, cancellationToken);
    }
}