using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class HubRepository(ModelDbContext context)
{
    public IQueryable<Hub> Get()
    {
        return context.Hubs;
    }

    public Task<Hub?> GetAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == warehouse.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == parkingSpot.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h=> h.Id == bay.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == adminStaff.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == bayStaff.HubId, cancellationToken);
    }

    public Task<Hub?> GetAsync(Picker picker, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == picker.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == stuffer.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(Load load, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == load.HubId, cancellationToken);
    }
    
    public Task<Hub?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get().FirstOrDefaultAsync(h => h.Id == trip.HubId, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return Get().CountAsync(cancellationToken);
    }
    
    public async Task AddAsync(Hub hub, CancellationToken cancellationToken)
    {
        await context.Hubs.AddAsync(hub, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task AddAsync(Hub hub, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        hub.Shifts.Remove(operatingHour);
        hub.Shifts.Add(operatingHour);
        operatingHour.Hub = hub;
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountOperatingAsync(TimeSpan time, CancellationToken cancellationToken)
    {
        return Get()
            .Where(h => h.Shifts
                .Any(oh => oh.StartTime <= time && 
                           oh.StartTime + oh.Duration >= time))
            .CountAsync(cancellationToken);
    }
}