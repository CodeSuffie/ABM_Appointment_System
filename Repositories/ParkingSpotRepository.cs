using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class ParkingSpotRepository(ModelDbContext context)
{
    public IQueryable<ParkingSpot> Get()
    {
        return context.ParkingSpots;
    }
    
    public Task<ParkingSpot?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(ps => ps.TripId == trip.Id, cancellationToken);
    }

    public async Task AddAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        await context.ParkingSpots.AddAsync(parkingSpot, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(ParkingSpot parkingSpot, Hub hub, CancellationToken cancellationToken)
    {
        parkingSpot.Hub = hub;
        hub.ParkingSpots.Remove(parkingSpot);
        hub.ParkingSpots.Add(parkingSpot);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bayCount = await context.ParkingSpots
            .CountAsync(ps => ps.HubId == hub.Id, cancellationToken);

        return bayCount;
    }

    public Task<int> CountUnclaimedAsync(CancellationToken cancellationToken)
    {
        return Get()
            .Where(ps => ps.Trip == null)
            .CountAsync(cancellationToken);
    }
}