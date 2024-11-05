using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class ParkingSpotService(ModelDbContext context)
{
    public static async Task InitializeObjectAsync(Hub hub, int i, CancellationToken cancellationToken)
    {
        var location = new Location
        {
            LocationType = LocationType.ParkingSpot,
            XLocation = AgentConfig.ParkingSpotLocations[i, 0],
            YLocation = AgentConfig.ParkingSpotLocations[i, 1],
        };
        
        var parkingSpot = new ParkingSpot
        {
            Hub = hub,
            Location = location
        };
            
        hub.ParkingSpots.Add(parkingSpot);
    }

    public async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.ParkingSpotLocations.Length; i++)
        {
            await InitializeObjectAsync(hub, i, cancellationToken);
        }
    }
}