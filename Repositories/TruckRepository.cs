using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TruckRepository(ModelDbContext context)
{
    public Task<IQueryable<Truck>> GetAsync(CancellationToken cancellationToken)
    {
        var trucks = context.Trucks;

        return Task.FromResult<IQueryable<Truck>>(trucks);
    }
    
    public async Task<Truck?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await context.Trucks
            .FirstOrDefaultAsync(t => t.TripId == trip.Id, cancellationToken);

        return truck;
    }

    
    public async Task AddAsync(Truck truck, CancellationToken cancellationToken)
    {
        await context.Trucks
            .AddAsync(truck, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}