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
            logger.LogError("Work \n({@Work})\n does not have a Duration",
                work);

            return false;
        }
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        
        return endTime <= modelState.ModelTime;
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

        await workRepository.AddAsync(work, trip, cancellationToken);
    }
    
    public async Task AddAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = GetNew(WorkType.CheckIn);

        await workRepository.AddAsync(work, trip, adminStaff, cancellationToken);
    }

    public async Task AddAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = GetNew(WorkType.Bay);

        await workRepository.AddAsync(work, trip, bay, cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, BayStaff bayStaff, Pellet pellet, WorkType workType, CancellationToken cancellationToken)
    {
        var work = GetNew(workType);

        await workRepository.AddAsync(work, bay, bayStaff, pellet, cancellationToken);
    }
}