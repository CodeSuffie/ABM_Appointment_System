using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class WarehouseRepository(ModelDbContext context)
{
    public IQueryable<Warehouse> Get()
    {
        var warehouses = context.Warehouses;

        return warehouses;
    }
    
    public Task<Warehouse?> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var warehouse = Get()
            .FirstOrDefaultAsync(w => w.HubId == hub.Id,
                cancellationToken);

        return warehouse;
    }

    public Task<Warehouse?> GetAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        var warehouse = Get()
            .FirstOrDefaultAsync(w => w.Id == pellet.WarehouseId,
                cancellationToken);

        return warehouse;
    }
    
    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        await context.Warehouses.AddAsync(warehouse, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(Warehouse warehouse, Hub hub, CancellationToken cancellationToken)
    {
        warehouse.Hub = hub;
        hub.Warehouse = warehouse;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}