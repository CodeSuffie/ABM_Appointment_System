using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class PelletRepository(ModelDbContext context)
{
    public IQueryable<Pellet> Get()
    {
        return context.Pellets;
    }

    public IQueryable<Pellet> Get(Load load)
    {
        return Get()
            .Where(p => p.LoadId == load.Id);
    }
    
    public IQueryable<Pellet> Get(TruckCompany truckCompany)
    {
        return Get()
            .Where(p => p.TruckCompanyId == truckCompany.Id);
    }
    
    public IQueryable<Pellet> Get(Truck truck)
    {
        return Get()
            .Where(p => p.TruckId == truck.Id);
    }
    
    public IQueryable<Pellet> Get(Bay bay)
    {
        return Get()
            .Where(p => p.BayId == bay.Id);
    }
    
    public IQueryable<Pellet> Get(Warehouse warehouse)
    {
        return Get()
            .Where(p => p.WarehouseId == warehouse.Id);
    }

    public Task<Pellet?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(p => p.Id == work.PelletId, cancellationToken);
    }
    
    public IQueryable<Pellet> GetUnclaimed(TruckCompany truckCompany)
    {
        return Get(truckCompany)
            .Where(p => p.Load == null);
    }

    public IQueryable<Pellet> GetUnclaimed(Warehouse warehouse)
    {
        return Get(warehouse)
            .Where(p => p.Load == null);
    }

    public async Task AddAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        await context.Pellets.AddAsync(pellet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Load load, CancellationToken cancellationToken)
    {
        pellet.Load = load;
        load.Pellets.Remove(pellet);
        load.Pellets.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pellet.TruckCompany = truckCompany;
        truckCompany.Inventory.Remove(pellet);
        truckCompany.Inventory.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Truck truck, CancellationToken cancellationToken)
    {
        pellet.Truck = truck;
        truck.Inventory.Remove(pellet);
        truck.Inventory.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = bay;
        bay.Inventory.Remove(pellet);
        bay.Inventory.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pellet pellet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pellet.Warehouse = warehouse;
        warehouse.Inventory.Remove(pellet);
        warehouse.Inventory.Add(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pellet.TruckCompany = null;
        truckCompany.Inventory.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Truck truck, CancellationToken cancellationToken)
    {
        pellet.Truck = null;
        truck.Inventory.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Load load, CancellationToken cancellationToken)
    {
        pellet.Load = null;
        load.Pellets.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        pellet.Bay = null;
        bay.Inventory.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pellet pellet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pellet.Warehouse = null;
        warehouse.Inventory.Remove(pellet);
        
        return context.SaveChangesAsync(cancellationToken);
    }
}