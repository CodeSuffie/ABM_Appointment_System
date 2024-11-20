using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class HubRepository(ModelDbContext context)
{
    public async Task<DbSet<Hub>> GetHubsAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs;

        return hubs;
    }
    
    public async Task<Hub?> GetHubByParkingSpotAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == parkingSpot.HubId, cancellationToken);

        return hub;
    }
    
    public async Task<Hub?> GetHubByLoadAsync(Load load, CancellationToken cancellationToken)
    {
        if (load.HubId == null) return null;
        
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == load.HubId, cancellationToken);

        return hub;
    }
    
    public async Task<Hub?> GetHubByBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h=> h.Id == bay.HubId, cancellationToken);

        // if (hub == null)
        //     throw new Exception("There was no Hub assigned to this Bay.");
        
        return hub;
    }
    
    public async Task<Hub> GetHubByStaffAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == adminStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This AdminStaff did not have a Hub assigned.");

        return hub;
    }
    
    public async Task<Hub> GetHubByStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == bayStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This BayStaff did not have a Hub assigned.");

        return hub;
    }
    
    public async Task AddAsync(Hub hub, CancellationToken cancellationToken)
    {
        await context.Hubs
            .AddAsync(hub, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}