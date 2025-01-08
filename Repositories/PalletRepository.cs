using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;

namespace Repositories;

public sealed class PalletRepository(
    ModelDbContext context,
    Instrumentation instrumentation)
{
    public IQueryable<Pallet> Get()
    {
        return context.Pallets;
    }

    public IQueryable<Pallet> Get(Load load)
    {
        return Get()
            .Where(p => p.LoadId == load.Id);
    }
    
    public IQueryable<Pallet> Get(TruckCompany truckCompany)
    {
        return Get()
            .Where(p => p.TruckCompanyId == truckCompany.Id);
    }
    
    public IQueryable<Pallet> Get(Truck truck)
    {
        return Get()
            .Where(p => p.TruckId == truck.Id);
    }
    
    public IQueryable<Pallet> Get(Bay bay)
    {
        return Get()
            .Where(p => p.BayId == bay.Id);
    }
    
    public IQueryable<Pallet> Get(Warehouse warehouse)
    {
        return Get()
            .Where(p => p.WarehouseId == warehouse.Id);
    }

    public Task<Pallet?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(p => p.Id == work.PalletId, cancellationToken);
    }
    
    public IQueryable<Pallet> GetUnclaimed(TruckCompany truckCompany)
    {
        return Get(truckCompany)
            .Where(p => p.Load == null);
    }

    public IQueryable<Pallet> GetUnclaimed(Warehouse warehouse)
    {
        return Get(warehouse)
            .Where(p => p.Load == null);
    }

    public async Task AddAsync(Pallet pallet, CancellationToken cancellationToken)
    {
        await context.Pallets.AddAsync(pallet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pallet pallet, Load load, CancellationToken cancellationToken)
    {
        pallet.Load = load;
        load.Pallets.Remove(pallet);
        load.Pallets.Add(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pallet pallet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pallet.TruckCompany = truckCompany;
        truckCompany.Inventory.Remove(pallet);
        truckCompany.Inventory.Add(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pallet pallet, Truck truck, CancellationToken cancellationToken)
    {
        pallet.Truck = truck;
        truck.Inventory.Remove(pallet);
        truck.Inventory.Add(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pallet pallet, Bay bay, CancellationToken cancellationToken)
    {
        pallet.Bay = bay;
        var removed = bay.Inventory.Remove(pallet);
        bay.Inventory.Add(pallet);

        if (!removed)
        {
            instrumentation.Add(Metric.PalletBay, 1, ("Pallet", pallet.Id), ("Bay", bay.Id));
        }
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task SetAsync(Pallet pallet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pallet.Warehouse = warehouse;
        warehouse.Inventory.Remove(pallet);
        warehouse.Inventory.Add(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pallet pallet, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        pallet.TruckCompany = null;
        truckCompany.Inventory.Remove(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pallet pallet, Truck truck, CancellationToken cancellationToken)
    {
        pallet.Truck = null;
        truck.Inventory.Remove(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pallet pallet, Load load, CancellationToken cancellationToken)
    {
        pallet.Load = null;
        load.Pallets.Remove(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pallet pallet, Bay bay, CancellationToken cancellationToken)
    {
        pallet.Bay = null;
        var removed = bay.Inventory.Remove(pallet);
        
        if (removed)
        {
            instrumentation.Add(Metric.PalletBay, -1, ("Pallet", pallet.Id), ("Bay", bay.Id));
        }
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task UnsetAsync(Pallet pallet, Warehouse warehouse, CancellationToken cancellationToken)
    {
        pallet.Warehouse = null;
        warehouse.Inventory.Remove(pallet);
        
        return context.SaveChangesAsync(cancellationToken);
    }
}