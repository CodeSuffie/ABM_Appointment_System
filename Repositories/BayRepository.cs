using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class BayRepository(ModelDbContext context)
{
    public IQueryable<Bay> Get()
    {
        var bays = context.Bays;
        
        return bays;
    }
    
    public IQueryable<Bay> Get(Hub hub)
    {
        var bays = Get()
            .Where(b => b.HubId == hub.Id);
        
        return bays;
    }
    
    public async Task<Bay?> GetAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var bay = await Get()
            .FirstOrDefaultAsync(b => b.Id == bayShift.BayId, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var bay = await Get()
            .FirstOrDefaultAsync(b => b.TripId == trip.Id, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.BayId == null) return null;
        
        var bay = await Get()
            .FirstOrDefaultAsync(b => b.Id == work.BayId, cancellationToken);

        return bay;
    }
    
    public async Task<Bay?> GetAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        if (pellet.BayId == null) return null;
        
        var bay = await Get()
            .FirstOrDefaultAsync(b => b.Id == pellet.BayId, cancellationToken);

        return bay;
    }

    public async Task<int> CountAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bayCount = await Get()
            .CountAsync(b => b.HubId == hub.Id, cancellationToken);

        return bayCount;
    }
    
    public async Task SetAsync(Bay bay, Hub hub, CancellationToken cancellationToken)
    {
        bay.Hub = hub;
        hub.Bays.Add(bay);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Bay bay, BayStatus status, CancellationToken cancellationToken)
    {
        bay.BayStatus = status;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(BayStatus bayStatus, CancellationToken cancellationToken)
    {
        return Get()
            .Where(b => b.BayStatus == bayStatus)
            .CountAsync(cancellationToken);
    }
}