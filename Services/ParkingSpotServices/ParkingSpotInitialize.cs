using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotInitialize(
    ILogger<ParkingSpotInitialize> logger,
    ParkingSpotService parkingSpotService,
    LocationService locationService,
    HubRepository hubRepository,
    ParkingSpotRepository parkingSpotRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = parkingSpotService.GetNewObject(hub);
        
            logger.LogDebug("Setting location for this ParkingSpot ({@ParkingSpot})...",
                parkingSpot);
            await locationService.InitializeObjectAsync(parkingSpot, cancellationToken);

            logger.LogDebug("Setting ParkingSpot ({@ParkingSpot}) to its Hub ({@Hub})...",
                parkingSpot,
                hub);
            await parkingSpotRepository.SetAsync(parkingSpot, hub, cancellationToken);
            logger.LogInformation("New ParkingSpot created: ParkingSpot={@ParkingSpot}", parkingSpot);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.ParkingSpotLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}