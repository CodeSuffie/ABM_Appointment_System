using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class LoadRepository(
    ModelDbContext context,
    BayRepository bayRepository)
{
    public IQueryable<Load> GetByStart(TruckCompany truckCompany)
    {
        var loads = context.Loads
            .Where(l => l.TruckCompanyStartId == truckCompany.Id);

        return loads;
    }
    
    public IQueryable<Load> GetByEnd(TruckCompany truckCompany)
    {
        var loads = context.Loads
            .Where(l => l.TruckCompanyEndId == truckCompany.Id);

        return loads;
    }
    
    public IQueryable<Load> Get(Hub hub)
    {
        var loads = context.Loads
            .Where(l => l.HubId == hub.Id);

        return loads;
    }
    
    public Task<Load?> GetPickUpAsync(Trip trip, CancellationToken cancellationToken)
    {
        return context.Loads.FirstOrDefaultAsync(l => l.PickUpTripId == trip.Id, cancellationToken);
    }
    
    public Task<Load?> GetDropOffAsync(Trip trip, CancellationToken cancellationToken)
    {
        return context.Loads.FirstOrDefaultAsync(l => l.DropOffTripId == trip.Id, cancellationToken);
    }
    
    public IQueryable<Load> GetUnclaimedDropOff(TruckCompany truckCompany)
    {
        var dropOffs = GetByStart(truckCompany)
            .Where(l => l.DropOffTrip == null);

        return dropOffs;
    }
    
    public IQueryable<Load> GetUnclaimedPickUp(TruckCompany truckCompany)
    {
        var pickUps = GetByEnd(truckCompany)
            .Where(l => l.PickUpTrip == null);

        return pickUps;
    }
    
    public IQueryable<Load> GetUnclaimedPickUp(Hub hub, TruckCompany truckCompany)
    {
        var pickUps = GetUnclaimedPickUp(truckCompany)
            .Where(l => l.HubId == hub.Id);

        return pickUps;
    }
    
    public async Task AddAsync(Load load, CancellationToken cancellationToken)
    {
        await context.Loads
            .AddAsync(load, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(Load load, CancellationToken cancellationToken)
    {
        context.Loads.Remove(load);
        return context.SaveChangesAsync(cancellationToken);
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

    public Task SetAsync(Load load, LoadType loadType, CancellationToken cancellationToken)
    {
        load.LoadType = loadType;
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task UnsetAsync(Load load, Bay bay, CancellationToken cancellationToken)
    {
        load.Bay = null;
        bay.Loads.Remove(load);
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task UnsetPickUpAsync(Load load, Trip trip, CancellationToken cancellationToken)
    {
        load.PickUpTrip = null;
        trip.PickUp = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }
}