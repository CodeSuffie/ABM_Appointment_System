using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class LoadRepository(
    ModelDbContext context,
    BayRepository bayRepository)
{
    public IQueryable<Load> Get()
    {
        var loads = context.Loads;

        return loads;
    }
    
    public IQueryable<Load> Get(TruckCompany truckCompany)
    {
        var loads = Get()
            .Where(l => l.TruckCompanyId == truckCompany.Id);

        return loads;
    }
    
    public IQueryable<Load> Get(Hub hub)
    {
        var loads = Get()
            .Where(l => l.HubId == hub.Id);

        return loads;
    }
    
    public IQueryable<Load> Get(Trip trip)
    {
        var load = Get()
            .Where(l => l.TripId == trip.Id);

        return load;
    }
    
    public async Task<Load?> GetAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        var load = await Get()
            .FirstOrDefaultAsync(l => pellet.LoadId == l.Id, cancellationToken);
            
        return load;
    }
    
    public async Task<Load?> GetAsync(Trip trip, LoadType loadType, CancellationToken cancellationToken)
    {
        var load = await Get(trip)
            .FirstOrDefaultAsync(l => l.LoadType == loadType,
                cancellationToken);

        return load;
    }
    
    public async Task AddAsync(Load load, CancellationToken cancellationToken)
    {
        await context.Loads
            .AddAsync(load, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Load load, LoadType loadType, CancellationToken cancellationToken)
    {
        load.LoadType = loadType;
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountUnclaimedAsync(CancellationToken cancellationToken)
    {
        return context.Loads
            .Where(l => l.Trip == null)
            .CountAsync(cancellationToken);
    }
}