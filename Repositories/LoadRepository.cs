using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class LoadRepository(
    ModelDbContext context,
    BayRepository bayRepository)
{
    public async Task<IQueryable<Load>> GetByStartAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var loads = context.Loads
            .Where(l => l.TruckCompanyStartId == truckCompany.Id);

        return loads;
    }
    
    public async Task<IQueryable<Load>> GetByEndAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var loads = context.Loads
            .Where(l => l.TruckCompanyEndId == truckCompany.Id);

        return loads;
    }
    
    public async Task<IQueryable<Load>> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var loads = context.Loads
            .Where(l => l.HubId == hub.Id);

        return loads;
    }
    
    public async Task<Load?> GetPickUpAsync(Trip trip, CancellationToken cancellationToken)
    {
        var load = await context.Loads
            .FirstOrDefaultAsync(l => l.PickUpTripId == trip.Id, cancellationToken);

        return load;
    }
    
    public async Task<Load?> GetDropOffAsync(Trip trip, CancellationToken cancellationToken)
    {
        var load = await context.Loads
            .FirstOrDefaultAsync(l => l.DropOffTripId == trip.Id, cancellationToken);

        return load;
    }
    
    public async Task<IQueryable<Load>> GetUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = (await GetByStartAsync(truckCompany, cancellationToken))
            .Where(l => l.DropOffTrip == null);

        return dropOffs;
    }
    
    public async Task<IQueryable<Load>> GetUnclaimedPickUpAsync(CancellationToken cancellationToken)
    {
        var pickUps = context.Loads
            .Where(l => l.PickUpTrip == null);

        return pickUps;
    }
    
    public async Task<IQueryable<Load>> GetUnclaimedPickUpAsync(Hub hub, CancellationToken cancellationToken)
    {
        var pickUps = (await GetAsync(hub, cancellationToken))
            .Where(l => l.PickUpTrip == null);

        return pickUps;
    }
    
    public async Task AddAsync(Load load, CancellationToken cancellationToken)
    {
        await context.Loads
            .AddAsync(load, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Load load, Bay bay, CancellationToken cancellationToken)
    {
        var oldBay = await bayRepository.GetAsync(load, cancellationToken);
        if (oldBay != null)
        {
            await UnsetAsync(load, oldBay, cancellationToken);
        }

        load.Bay = bay;
        bay.Loads.Add(load);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UnsetAsync(Load load, Bay bay, CancellationToken cancellationToken)
    {
        load.Bay = null;
        bay.Loads.Remove(load);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}