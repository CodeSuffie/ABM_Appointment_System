using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class ParkingSpotService(ModelDbContext context)
{
    public static async Task InitializeObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot {
            Hub = hub
        };
        
        // TODO: Add Location
            
        hub.ParkingSpots.Add(parkingSpot);
    }

    public async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.ParkingSpotCountPerHub; i++)
        {
            await InitializeObjectAsync(hub, cancellationToken);
        }
    }
}