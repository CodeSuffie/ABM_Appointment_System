using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.TripServices;

public sealed class TripStepper(
    TripRepository tripRepository,
    WorkRepository workRepository,
    TripService tripService) : IStepperService<Trip>
{
    public async Task StepAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work == null) return;

        if (work.WorkType == WorkType.TravelHub)
        {
            await tripService.TravelHubAsync(trip, cancellationToken);
            if (await tripService.IsAtHubAsync(trip, cancellationToken))
            {
                await tripService.AlertTravelHubCompleteAsync(trip, cancellationToken);
            }
        }

        if (work.WorkType == WorkType.TravelHome)
        {
            await tripService.TravelHomeAsync(trip, cancellationToken);
            if (await tripService.IsAtHomeAsync(trip, cancellationToken))
            {
                await tripService.AlertTravelHomeCompleteAsync(trip, cancellationToken);
            }
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trips = (await tripRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            await StepAsync(trip, cancellationToken);
        }
    }
}