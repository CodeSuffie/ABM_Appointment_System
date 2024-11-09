using Database;
using Database.Models;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(ModelDbContext context)
{
    public async Task<ParkingSpot> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        return parkingSpot;
    }

    
}