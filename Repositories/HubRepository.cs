using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class HubRepository(ModelDbContext context)
{
    public async Task<IQueryable<Hub>> GetAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs;

        return hubs;
    }
    
    public async Task<Hub?> GetAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == parkingSpot.HubId, cancellationToken);

        return hub;
    }
    
    public async Task<Hub?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h=> h.Id == bay.HubId, cancellationToken);

        // if (hub == null)
        //     throw new Exception("There was no Hub assigned to this Bay.");
        
        return hub;
    }
    
    public async Task<Hub> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == adminStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This AdminStaff did not have a Hub assigned.");

        return hub;
    }
    
    public async Task<Hub> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == bayStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This BayStaff did not have a Hub assigned.");

        return hub;
    }
    
    public async Task<Hub?> GetAsync(Load load, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == load.HubId, cancellationToken);

        return hub;
    }
    
    public async Task<Hub?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == trip.HubId, cancellationToken);

        return hub;
    }
    
    public async Task AddAsync(Hub hub, CancellationToken cancellationToken)
    {
        await context.Hubs
            .AddAsync(hub, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}