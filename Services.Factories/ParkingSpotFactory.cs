using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class ParkingSpotFactory(
    ILogger<ParkingSpotFactory> logger,
    ParkingSpotRepository parkingSpotRepository,
    LocationService locationService,
    ModelState modelState) : IFactoryService<ParkingSpot>
{
    public async Task<ParkingSpot?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot();
        
        await parkingSpotRepository.AddAsync(parkingSpot, cancellationToken);

        return parkingSpot;
    }
    
    public async Task<ParkingSpot?> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = await GetNewObjectAsync(cancellationToken);
        if (parkingSpot == null)
        {
            logger.LogError("ParkingSpot could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting ParkingSpot \n({@ParkingSpot})\n to its Hub \n({@Hub})",
            parkingSpot,
            hub);
        await parkingSpotRepository.SetAsync(parkingSpot, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this ParkingSpot \n({@ParkingSpot})",
            parkingSpot);
        await locationService.InitializeObjectAsync(parkingSpot, cancellationToken);

        return parkingSpot;
    }
}