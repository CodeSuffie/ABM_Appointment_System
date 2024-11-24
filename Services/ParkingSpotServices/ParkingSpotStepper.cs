using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;
using Services.TripServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotStepper(
    ParkingSpotService parkingSpotService,
    ParkingSpotRepository parkingSpotRepository,
    TripRepository tripRepository) : IStepperService<ParkingSpot>
{
    public async Task StepAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(parkingSpot, cancellationToken);
        
        if (trip == null)
        {
            await parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var parkingSpots = (parkingSpotRepository.Get())
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var parkingSpot in parkingSpots)
        {
            await StepAsync(parkingSpot, cancellationToken);
        }
    }
}