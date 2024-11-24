using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class WorkService(
    WorkRepository workRepository,
    ModelState modelState)
{
    public Task<bool> IsWorkCompletedAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.Duration == null) return Task.FromResult(false);
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        
        return Task.FromResult(endTime <= modelState.ModelTime);
    }
    
    public async Task AdaptWorkLoadAsync(Bay bay, CancellationToken cancellationToken)
    {
        WorkType? workType = bay.BayStatus switch
        {
            BayStatus.DroppingOffStarted or BayStatus.FetchStarted or BayStatus.FetchFinished => WorkType.DropOff,
            BayStatus.PickUpStarted => WorkType.PickUp,
            _ => null
        };

        if (workType == null) return;

        var works = (await workRepository.GetAsync(bay, (WorkType) workType, cancellationToken))
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

        await foreach (var work in works)
        {
            await workRepository.SetDurationAsync(work, (totalDuration / count), cancellationToken);
        }
    }
    
    public Task<TimeSpan?> GetTimeAsync(WorkType workType, CancellationToken cancellationToken)
    {
        return Task.FromResult<TimeSpan?>(workType switch
        {
            WorkType.CheckIn => modelState.ModelConfig.CheckInWorkTime,
            WorkType.DropOff => modelState.ModelConfig.DropOffWorkTime,
            WorkType.PickUp => modelState.ModelConfig.PickUpWorkTime,
            WorkType.Fetch => modelState.ModelConfig.FetchWorkTime,
            _ => null
        });
    }
    
    private async Task<Work> GetNewAsync(WorkType workType, CancellationToken cancellationToken)
    {
        var duration = await GetTimeAsync(workType, cancellationToken);
        
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
        var work = await GetNewAsync(workType, cancellationToken);
        work.Trip = trip;

        await workRepository.AddAsync(work, cancellationToken);
    }
    
    public async Task AddAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(WorkType.CheckIn, cancellationToken);
        work.Trip = trip;
        work.AdminStaff = adminStaff;
        
        trip.Work = work;
        adminStaff.Work = work;

        await workRepository.AddAsync(work, cancellationToken);
    }

    public async Task AddAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(WorkType.Bay, cancellationToken);
        work.Trip = trip;
        work.Bay = bay;
        
        trip.Work = work;
        bay.Works.Add(work);

        await workRepository.AddAsync(work, cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, BayStaff bayStaff, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewAsync(workType, cancellationToken);
        work.BayStaff = bayStaff;
        work.Bay = bay;
        
        bayStaff.Work = work;
        bay.Works.Add(work);

        await workRepository.AddAsync(work, cancellationToken);
    }
}