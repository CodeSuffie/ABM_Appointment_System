using Database;
using Services.Abstractions;
using Settings;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotInitialize(
    ModelDbContext context,
    ParkingSpotService parkingSpotService,
    LocationService locationService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = await parkingSpotService.GetNewObjectAsync(hub, cancellationToken);
        
            await locationService.InitializeObjectAsync(parkingSpot, cancellationToken);
            
            hub.ParkingSpots.Add(parkingSpot);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.ParkingSpotLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}