using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TripServices;

public sealed class TripStepper(
    ILogger<TripStepper> logger,
    TripRepository tripRepository,
    WorkRepository workRepository,
    TripService tripService,
    ModelState modelState) : IStepperService<Trip>
{
    public async Task StepAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work == null)
        {
            logger.LogInformation("Trip ({@Trip}) has no active Work assigned in this Step ({Step}).",
                trip,
                modelState.ModelTime);
            
            logger.LogDebug("Trip ({@Trip}) will remain idle in this Step ({Step})...",
                trip,
                modelState.ModelTime);
            
            return;
        }

        if (work.WorkType == WorkType.TravelHub)
        {
            logger.LogInformation("Trip ({@Trip}) has Work ({@Work}) assigned of Type {WorkType} in this Step ({Step}).",
                trip,
                work,
                WorkType.TravelHub,
                modelState.ModelTime);
            
            logger.LogDebug("Travelling to the Hub for this Trip ({@Trip}) in this Step ({Step}).",
                trip,
                modelState.ModelTime);
            await tripService.TravelHubAsync(trip, cancellationToken);
            
            if (await tripService.IsAtHubAsync(trip, cancellationToken))
            {
                logger.LogInformation("Trip ({@Trip}) has arrived at the Hub in this Step ({Step}).",
                    trip,
                    modelState.ModelTime);
                
                logger.LogDebug("Alerting Travel to Hub Complete for this Trip ({@Trip}) in this Step ({Step}).",
                    trip,
                    modelState.ModelTime);
                await tripService.AlertTravelHubCompleteAsync(trip, cancellationToken);
            }
        }

        else if (work.WorkType == WorkType.TravelHome)
        {
            logger.LogInformation("Trip ({@Trip}) has Work ({@Work}) assigned of Type {WorkType} in this Step ({Step}).",
                trip,
                work,
                WorkType.TravelHome,
                modelState.ModelTime);
            
            logger.LogDebug("Travelling home for this Trip ({@Trip}) in this Step ({Step}).",
                trip,
                modelState.ModelTime);
            await tripService.TravelHomeAsync(trip, cancellationToken);
            
            if (await tripService.IsAtHomeAsync(trip, cancellationToken))
            {
                logger.LogInformation("Trip ({@Trip}) has arrived home in this Step ({Step}).",
                    trip,
                    modelState.ModelTime);
                
                logger.LogDebug("Alerting Travel home Complete for this Trip ({@Trip}) in this Step ({Step}).",
                    trip,
                    modelState.ModelTime);
                await tripService.AlertTravelHomeCompleteAsync(trip, cancellationToken);
            }
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trips = tripRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            logger.LogDebug("Handling Step ({Step}) for this Trip ({@Trip})...",
                modelState.ModelTime,
                trip);
            
            await StepAsync(trip, cancellationToken);
            
            logger.LogDebug("Completed handling Step ({Step}) for this Trip ({@Trip}).",
                modelState.ModelTime,
                trip);
        }
    }
}