using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class LoadRepository(ModelDbContext context)
{
    public IQueryable<Load> Get()
    {
        return context.Loads
            .Include(l => l.Pallets);
    }
    
    public IQueryable<Load> Get(TruckCompany truckCompany)
    {
        return Get()
            .Where(l => l.TruckCompanyId == truckCompany.Id);
    }
    
    public IQueryable<Load> Get(Hub hub)
    {
        return Get()
            .Where(l => l.HubId == hub.Id);
    }
    
    public IQueryable<Load> Get(Trip trip)
    {
        return Get()
            .Where(l => l.TripId == trip.Id);
    }
    
    public Task<Load?> GetAsync(Trip trip, LoadType loadType, CancellationToken cancellationToken)
    {
        return Get(trip)
            .FirstOrDefaultAsync(l => l.LoadType == loadType, cancellationToken);
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

    public Task SetAsync(Load load, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        load.TruckCompany = truckCompany;
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Load load, Hub hub, CancellationToken cancellationToken)
    {
        load.Hub = hub;
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(Load load, CancellationToken cancellationToken)
    {
        context.Loads
            .Remove(load);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountUnclaimedAsync(CancellationToken cancellationToken)
    {
        return context.Loads
            .Where(l => l.Trip == null)
            .CountAsync(cancellationToken);
    }
}