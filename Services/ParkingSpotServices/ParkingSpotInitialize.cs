using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotInitialize(
    ParkingSpotService parkingSpotService,
    LocationService locationService,
    HubRepository hubRepository,
    ParkingSpotRepository parkingSpotRepository) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = (await hubRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = await parkingSpotService.GetNewObjectAsync(hub, cancellationToken);
        
            await locationService.InitializeObjectAsync(parkingSpot, cancellationToken);

            await parkingSpotRepository.SetAsync(parkingSpot, hub, cancellationToken);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.ParkingSpotLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}