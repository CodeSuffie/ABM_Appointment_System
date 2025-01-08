using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;

namespace Repositories;

public sealed class BayRepository(
    ModelDbContext context,
    Instrumentation instrumentation)
{
    public IQueryable<Bay> Get()
    {
        var bays = context.Bays
            .Include(t => t.Inventory)
            .Include(t => t.Appointments);
        
        return bays;
    }
    
    public IQueryable<Bay> Get(Hub hub)
    {
        return Get()
            .Where(b => b.HubId == hub.Id);
    }
    
    public Task<Bay?> GetAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(b => b.Id == bayShift.BayId, cancellationToken);
    }
    
    public Task<Bay?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(b => b.TripId == trip.Id, cancellationToken);
    }
    
    public Task<Bay?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(b => b.Id == work.BayId, cancellationToken);
    }
    
    public Task<Bay?> GetAsync(Pallet pallet, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(b => b.Id == pallet.BayId, cancellationToken);
    }

    public Task<Bay?> GetAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(b => b.Id == appointment.BayId, cancellationToken);
    }

    public async Task AddAsync(Bay bay, CancellationToken cancellationToken)
    {
        await context.Bays.AddAsync(bay, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public Task AddAsync(Bay bay, BayFlags flag, CancellationToken cancellationToken)
    {
        if (!bay.BayFlags.HasFlag(BayFlags.DroppedOff) && flag.HasFlag(BayFlags.DroppedOff))
        {
            instrumentation.Add(Metric.BayDroppedOff, 1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        if (!bay.BayFlags.HasFlag(BayFlags.Fetched) && flag.HasFlag(BayFlags.Fetched))
        {
            instrumentation.Add(Metric.BayFetched, 1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        if (!bay.BayFlags.HasFlag(BayFlags.PickedUp) && flag.HasFlag(BayFlags.PickedUp))
        {
            instrumentation.Add(Metric.BayPickedUp, 1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        
        bay.BayFlags |= flag;

        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task RemoveAsync(Bay bay, BayFlags flag, CancellationToken cancellationToken)
    {
        if (bay.BayFlags.HasFlag(BayFlags.DroppedOff) && flag.HasFlag(BayFlags.DroppedOff))
        {
            instrumentation.Add(Metric.BayDroppedOff, -1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        if (bay.BayFlags.HasFlag(BayFlags.Fetched) && flag.HasFlag(BayFlags.Fetched))
        {
            instrumentation.Add(Metric.BayFetched, -1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        if (bay.BayFlags.HasFlag(BayFlags.PickedUp) && flag.HasFlag(BayFlags.PickedUp))
        {
            instrumentation.Add(Metric.BayPickedUp, -1, ("Bay", bay.Id), ("BayFlag", flag));
        }
        
        bay.BayFlags &= ~flag;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task SetAsync(Bay bay, Hub hub, CancellationToken cancellationToken)
    {
        bay.Hub = hub;
        hub.Bays.Remove(bay);
        hub.Bays.Add(bay);
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task SetAsync(Bay bay, BayStatus status, CancellationToken cancellationToken)
    {
        bay.BayStatus = status;
        
        var change = status == BayStatus.Opened ? 1 : -1;
        instrumentation.Add(Metric.BayOpened, change, ("Bay", bay.Id));
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task SetAsync(Bay bay, BayFlags flags, CancellationToken cancellationToken)
    {
        bay.BayFlags = flags;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task<int> CountAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bayCount = Get()
            .CountAsync(b => b.HubId == hub.Id, cancellationToken);

        return bayCount;
    }

    public Task<int> CountAsync(bool hasTrip, CancellationToken cancellationToken)
    {
        return Get()
            .Where(b => (b.Trip != null) == hasTrip)
            .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(BayStatus bayStatus, CancellationToken cancellationToken)
    {
        return Get()
            .Where(b => b.BayStatus == bayStatus)
            .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(BayFlags bayFlag, CancellationToken cancellationToken)
    {
        return Get()
            .Where(b => b.BayFlags.HasFlag(bayFlag))
            .CountAsync(cancellationToken);
    }
}