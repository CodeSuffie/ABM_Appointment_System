using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.TripServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    ILogger<ParkingSpotService> logger,
    TripService tripService,
    HubRepository hubRepository)
{
    public ParkingSpot GetNewObject(Hub hub)
    {
        var parkingSpot = new ParkingSpot
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        return parkingSpot;
    }
    
    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null)
        {
            logger.LogError("ParkingSpot ({@ParkingSpot}) did not have a Hub assigned to alert free for.",
                parkingSpot);

            return;
        }
        
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitParking, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Hub ({@Hub}) did not have a Trip for this ParkingSpot ({@ParkingSpot}) to assign waiting for Check-In Work for.",
                hub,
                parkingSpot);
            
            logger.LogDebug("ParkingSpot ({@ParkingSpot}) will remain idle...",
                parkingSpot);
            
            return;
        }
        
        logger.LogDebug("Alerting Free for this ParkingSpot ({@ParkingSpot}) to selected Trip ({@Trip})...",
            parkingSpot,
            trip);
        await tripService.AlertFreeAsync(trip, parkingSpot, cancellationToken);
    }
}
