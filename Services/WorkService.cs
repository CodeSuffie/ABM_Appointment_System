using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services;

public sealed class WorkService(
    ModelRepository modelRepository,
    WorkRepository workRepository)
{
    public async Task<bool> IsWorkCompletedAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.Duration == null) return false;
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        var modelTime = await modelRepository.GetTimeAsync(cancellationToken);
        
        return endTime <= modelTime;
    }
    
    public async Task AdaptWorkLoadAsync(Bay bay, CancellationToken cancellationToken)
    {
        WorkType? workType = bay.BayStatus switch
        {
            BayStatus.DroppingOffStarted or BayStatus.FetchStarted or BayStatus.FetchFinished => WorkType.DropOff,
            BayStatus.PickUpStarted => WorkType.PickUp,
            _ => null
        };

        var works = (await workRepository.GetAsync(bay, WorkType.DropOff, cancellationToken))
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
}