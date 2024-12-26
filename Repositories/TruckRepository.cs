using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TruckRepository(ModelDbContext context)
{
    public IQueryable<Truck> Get()
    {
        return context.Trucks
            .Include(t => t.Inventory);
    }
    
    public Task<Truck?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(t => t.TripId == trip.Id, cancellationToken);
    }
    
    public async Task AddAsync(Truck truck, CancellationToken cancellationToken)
    {
        await context.Trucks
            .AddAsync(truck, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Truck truck, Pellet pellet, CancellationToken cancellationToken)
    {
        truck.Inventory.Remove(pellet);
        truck.Inventory.Add(pellet);
        pellet.Truck = truck;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RemoveAsync(Truck truck, Pellet pellet, CancellationToken cancellationToken)
    {
        truck.Inventory.Remove(pellet);
        pellet.Truck = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountUnclaimedAsync(CancellationToken cancellationToken)
    {
        return Get()
            .Where(t => t.Trip == null)
            .CountAsync(cancellationToken);
    }
}