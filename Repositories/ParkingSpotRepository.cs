using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class ParkingSpotRepository(ModelDbContext context)
{
    public async Task<ParkingSpot?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var parkingSpot = await context.ParkingSpots
            .FirstOrDefaultAsync(ps => ps.TripId == trip.Id, cancellationToken);

        return parkingSpot;
    }
    
    public async Task SetAsync(ParkingSpot parkingSpot, Hub hub, CancellationToken cancellationToken)
    {
        parkingSpot.Hub = hub;
        hub.ParkingSpots.Add(parkingSpot);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}