using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Services;

public sealed class LocationService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        // TODO: Find a location to assign to a new Hub which is not within the
        // TODO: ModelConfig.MinDistanceBetween range
    }
    
    public async Task InitializeObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        // TODO: Find a location to assign to a new TruckCompany which is not within the
        // TODO: ModelConfig.MinDistanceBetween range
    }
    
    public async Task InitializeObjectAsync(Bay bay, CancellationToken cancellationToken)
    {
        var bayCount = await context.Bays
            .CountAsync(x => x.HubId == bay.HubId, cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            bayCount, 
            AgentConfig.BayLocations.Length, 
            "Bay to set location for is not defined in the BayLocations Array");

        bay.XLocation = AgentConfig.BayLocations[bayCount, 0];
        bay.YLocation = AgentConfig.BayLocations[bayCount, 1];
    }
    
    public async Task InitializeObjectAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var parkingSpotCount = await context.ParkingSpots
            .CountAsync(x => x.HubId == parkingSpot.HubId, cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            parkingSpotCount, 
            AgentConfig.ParkingSpotLocations.Length, 
            "PerkingSpot to set location for is not defined in the ParkingSpotLocations Array");
        
        parkingSpot.XLocation = AgentConfig.ParkingSpotLocations[parkingSpotCount, 0];
        parkingSpot.YLocation = AgentConfig.ParkingSpotLocations[parkingSpotCount, 1];
    }
}