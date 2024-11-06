using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class ParkingSpotService(
    ModelDbContext context,
    LocationService locationService
    ) : IInitializationService
{
    private async Task<ParkingSpot> GetNewAgentAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot
        {
            Hub = hub,
        };

        var parkingSpotCount = await context.ParkingSpots
            .CountAsync(x => x.HubId == hub.Id, cancellationToken);
        
        await locationService.InitializeObjectAsync(parkingSpot, parkingSpotCount, cancellationToken);

        return parkingSpot;
    }

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = await GetNewAgentAsync(hub, cancellationToken);
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