using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class BayRepository(ModelDbContext context)
{
    public async Task<IQueryable<Bay>> GetBaysByHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = context.Bays
            .Where(x => x.HubId == hub.Id);
        
        return bays;
    }
    
    public async Task<Bay?> GetBayByShiftAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var bay = await context.Bays
            .FirstOrDefaultAsync(x => x.Id == bayShift.BayId, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetBayByTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        var bay = await context.Bays
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return bay;
    }
    
    public async Task<Bay?> GetBayByWorkAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.BayId == null) return null;
        
        var bay = await context.Bays
            .FirstOrDefaultAsync(x => x.Id == work.BayId, cancellationToken);

        return bay;
    }
    
    public async Task<Bay?> GetBayByLoadAsync(Load load, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get Bay for Load
    }
    
    public async Task SetBayHubAsync(Bay bay, Hub hub, CancellationToken cancellationToken)
    {
        bay.Hub = hub;
        hub.Bays.Add(bay);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetBayStatusAsync(Bay bay, BayStatus status, CancellationToken cancellationToken)
    {
        bay.BayStatus = status;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}