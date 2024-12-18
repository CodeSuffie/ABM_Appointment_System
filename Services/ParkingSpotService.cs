using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services;

public sealed class ParkingSpotService(
    ILogger<ParkingSpotService> logger,
    TripService tripService,
    HubRepository hubRepository)
{
    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null)
        {
            logger.LogError("ParkingSpot \n({@ParkingSpot})\n did not have a Hub assigned to alert free for.",
                parkingSpot);

            return;
        }
        
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitParking, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Hub \n({@Hub})\n did not have a Trip for this ParkingSpot \n({@ParkingSpot})\n to assign waiting for Check-In Work for.",
                hub,
                parkingSpot);
            
            logger.LogDebug("ParkingSpot \n({@ParkingSpot})\n will remain idle...",
                parkingSpot);
            
            return;
        }
        
        logger.LogDebug("Alerting Free for this ParkingSpot \n({@ParkingSpot})\n to selected Trip \n({@Trip})",
            parkingSpot,
            trip);
        await tripService.AlertFreeAsync(trip, parkingSpot, cancellationToken);
    }
}
