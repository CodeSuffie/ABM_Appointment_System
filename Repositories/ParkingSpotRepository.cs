using Database;
using Database.Models;

namespace Repositories;

public sealed class ParkingSpotRepository(ModelDbContext context)
{
    public async Task<ParkingSpot?> GetParkingSpotByTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get Parking Spot for Trip
    }
    
    public async Task SetParkingSpotHubAsync(ParkingSpot parkingSpot, Hub hub, CancellationToken cancellationToken)
    {
        parkingSpot.Hub = hub;
        hub.ParkingSpots.Add(parkingSpot);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}