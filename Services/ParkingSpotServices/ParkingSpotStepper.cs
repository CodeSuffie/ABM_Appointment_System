using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotStepper(
    ILogger<ParkingSpotStepper> logger,
    ParkingSpotService parkingSpotService,
    ParkingSpotRepository parkingSpotRepository,
    TripRepository tripRepository,
    ModelState modelState) : IStepperService<ParkingSpot>
{
    public async Task StepAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(parkingSpot, cancellationToken);

        if (trip != null)
        {
            logger.LogDebug("ParkingSpot ({@ParkingSpot}) has an active Trip assigned in this Step ({Step})...",
                parkingSpot,
                modelState.ModelTime);

            logger.LogDebug("ParkingSpot ({@ParkingSpot}) will remain idle in this Step ({Step})...",
                parkingSpot,
                modelState.ModelTime);

            return;
        }

        logger.LogInformation("ParkingSpot ({@ParkingSpot}) has no active Trip assigned in this Step ({Step}).",
            parkingSpot,
            modelState.ModelTime);
        
        logger.LogDebug("Alerting Free for this ParkingSpot ({@ParkingSpot}) in this Step ({Step}).",
            parkingSpot,
            modelState.ModelTime);
        await parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var parkingSpots = (parkingSpotRepository.Get())
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var parkingSpot in parkingSpots)
        {
            logger.LogDebug("Handling Step ({Step}) for this ParkingSpot ({@ParkingSpot})...",
                modelState.ModelTime,
                parkingSpot);
            
            await StepAsync(parkingSpot, cancellationToken);
            
            logger.LogDebug("Completed handling Step ({Step}) for this ParkingSpot ({@ParkingSpot}).",
                modelState.ModelTime,
                parkingSpot);
        }
    }
}