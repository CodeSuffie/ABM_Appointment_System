using Database;
using Database.Models;
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
    
    public async Task InitializeObjectAsync(Bay bay, int i, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            i, 
            AgentConfig.BayLocations.Length, 
            "Bay to set location for is not defined in the BayLocations Array");

        bay.XLocation = AgentConfig.BayLocations[i, 0];
        bay.YLocation = AgentConfig.BayLocations[i, 1];
    }
    
    public async Task InitializeObjectAsync(ParkingSpot parkingSpot, int i, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            i, 
            AgentConfig.ParkingSpotLocations.Length, 
            "PerkingSpot to set location for is not defined in the ParkingSpotLocations Array");
        
        parkingSpot.XLocation = AgentConfig.ParkingSpotLocations[i, 0];
        parkingSpot.YLocation = AgentConfig.ParkingSpotLocations[i, 1];
    }
}