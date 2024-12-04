using Database;
using Database.Models;

namespace Repositories;

public sealed class PelletRepository(ModelDbContext context)
{
    public IQueryable<Pellet> Get()
    {
        var pellets = context.Pellets;

        return pellets;
    }
    
    public IQueryable<Pellet> Get(TruckCompany truckCompany)
    {
        var pellets = Get()
            .Where(p => p.TruckCompany != null &&
                        p.TruckCompanyId == truckCompany.Id);

        return pellets;
    }
    
    public IQueryable<Pellet> Get(Hub hub)
    {
        var pellets = Get()
            .Where(p => p.Bay != null &&
                        hub.Bays.Any(b => p.BayId == b.Id));

        return pellets;
    }
    
    public IQueryable<Pellet> GetUnclaimed(TruckCompany truckCompany)
    {
        var pellets = Get(truckCompany)
            .Where(p => p.Load == null);

        return pellets;
    }

    public IQueryable<Pellet> GetUnclaimed(Hub hub)
    {
        var pellets = Get(hub)
            .Where(p => p.Load == null);

        return pellets;
    }

    public async Task AddAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        await context.Pellets.AddAsync(pellet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pellet.TruckCompany = truckCompany;
        truckCompany.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Load load, CancellationToken cancellationToken)
    {
        pellet.Load = load;
        load.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = bay;
        bay.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pellet.TruckCompany = null;
        truckCompany.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Load load, CancellationToken cancellationToken)
    {
        pellet.Load = load;
        load.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = bay;
        bay.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }
}