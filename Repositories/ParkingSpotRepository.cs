using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class ParkingSpotRepository(ModelDbContext context)
{
    public IQueryable<ParkingSpot> Get()
    {
        var parkingSpots = context.ParkingSpots;

        return parkingSpots;
    }
    
    public async Task<ParkingSpot?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var parkingSpot = await context.ParkingSpots
            .FirstOrDefaultAsync(ps => ps.TripId == trip.Id, cancellationToken);

        return parkingSpot;
    }

    public async Task<int> GetCountAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bayCount = await context.ParkingSpots
            .CountAsync(ps => ps.HubId == hub.Id, cancellationToken);

        return bayCount;
    }
    
    public async Task SetAsync(ParkingSpot parkingSpot, Hub hub, CancellationToken cancellationToken)
    {
        parkingSpot.Hub = hub;
        hub.ParkingSpots.Add(parkingSpot);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}