using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.TripServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    ILogger<ParkingSpotService> logger,
    TripService tripService,
    HubRepository hubRepository,
    LocationService locationService,
    ParkingSpotRepository parkingSpotRepository)
{
    public async Task<ParkingSpot> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        logger.LogDebug("Setting ParkingSpot ({@ParkingSpot}) to its Hub ({@Hub})...",
            parkingSpot,
            hub);
        await parkingSpotRepository.SetAsync(parkingSpot, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this ParkingSpot ({@ParkingSpot})...",
            parkingSpot);
        await locationService.InitializeObjectAsync(parkingSpot, cancellationToken);

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
