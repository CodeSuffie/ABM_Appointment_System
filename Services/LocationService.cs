using Database;
using Database.Models;

namespace Services;

public sealed class LocationService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        // TODO: Find a location to assign to a new Hub which is not within the
        // TODO: ModelConfig.MinDistanceBetween range
        
        var location = new Location
        {
            
        };
            
        hub.Location = location;
    }
    
    public async Task InitializeObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        // TODO: Find a location to assign to a new TruckCompany which is not within the
        // TODO: ModelConfig.MinDistanceBetween range
        
        var location = new Location
        {
            
        };
            
        truckCompany.Location = location;
    }
}