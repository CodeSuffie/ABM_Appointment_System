using Database;
using Database.Models;
using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class BayRepository(ModelDbContext context)
{
    public Task<IQueryable<Bay>> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = context.Bays
            .Where(b => b.HubId == hub.Id);
        
        return Task.FromResult(bays);
    }
    
    public async Task<Bay?> GetAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var bay = await context.Bays
            .FirstOrDefaultAsync(b => b.Id == bayShift.BayId, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var bay = await context.Bays
            .FirstOrDefaultAsync(b => b.TripId == trip.Id, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.BayId == null) return null;
        
        var bay = await context.Bays
            .FirstOrDefaultAsync(b => b.Id == work.BayId, cancellationToken);

        return bay;
    }
    
    public async Task<Bay?> GetAsync(Load load, CancellationToken cancellationToken)
    {
        if (load.BayId == null) return null;
        
        var bay = await context.Bays
            .FirstOrDefaultAsync(b => b.Id == load.BayId, cancellationToken);

        return bay;
    }

    public async Task<int> GetCountAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bayCount = await context.Bays
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

    public async Task AddAsync(Bay bay, BayLog log, CancellationToken cancellationToken)
    {
        bay.BayLogs.Add(log);
        log.Bay = bay;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}