using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

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
    
    public IQueryable<Pellet> Get(Bay bay)
    {
        var pellets = Get()
            .Where(p => p.BayId == bay.Id);

        return pellets;
    }
    
    public IQueryable<Pellet> Get(Warehouse warehouse)
    {
        var pellets = Get()
            .Where(p => p.WarehouseId == warehouse.Id);

        return pellets;
    }

    public async Task<Pellet?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        var pellet = await Get()
            .FirstOrDefaultAsync(p => p.Id == work.PelletId, cancellationToken);

        return pellet;
    }
    
    public IQueryable<Pellet> GetUnclaimed(TruckCompany truckCompany)
    {
        var pellets = Get(truckCompany)
            .Where(p => p.Loads.Count == 0);

        return pellets;
    }

    public IQueryable<Pellet> GetUnclaimed(Bay bay)
    {
        var pellets = Get(bay)
            .Where(p => p.Loads.Count == 0);

        return pellets;
    }

    public IQueryable<Pellet> GetUnclaimed(Warehouse warehouse)
    {
        var pellets = Get(warehouse)
            .Where(p => p.Loads.Count == 0);

        return pellets;
    }
    
    // public IQueryable<Pellet> GetUnclaimed(Load load)
    // {
    //     var pellets = Get(load)
    //         .Where(p => p.Work == null);
    //
    //     return pellets;
    // }

    public async Task AddAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        await context.Pellets.AddAsync(pellet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task AddAsync(Pellet pellet, Load load, CancellationToken cancellationToken)
    {
        pellet.Loads.Add(load);
        load.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pellet.TruckCompany = truckCompany;
        truckCompany.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = bay;
        bay.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pellet.Warehouse = warehouse;
        warehouse.Pellets.Add(pellet);
        
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
        pellet.Loads.Remove(load);
        load.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = null;
        bay.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pellet.Warehouse = null;
        warehouse.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetWorkAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        pellet.Work = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }
}