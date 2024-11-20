using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class LoadRepository(
    ModelDbContext context,
    BayRepository bayRepository)
{
    public async Task<List<Load>> GetUnclaimedDropOffLoadsByTruckCompanyAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = await context.Loads
            .Where(x => x.DropOffTrip == null)
            .Where(x => x.TruckCompanyStartId == truckCompany.Id)
            .ToListAsync(cancellationToken);

        return dropOffs;
    }
    
    public async Task<List<Load>> GetUnclaimedPickUpLoadsByHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var pickUps = await context.Loads
            .Where(x => x.PickUpTrip == null)
            .Where(x => x.HubId == hub.Id)
            .ToListAsync(cancellationToken);

        return pickUps;
    }
    
    public async Task<List<Load>> GetUnclaimedPickUpLoadsAsync(CancellationToken cancellationToken)
    {
        var pickUps = await context.Loads
            .Where(x => x.PickUpTrip == null)
            .ToListAsync(cancellationToken);

        return pickUps;
    }
    
    public async Task<Load?> GetPickUpLoadByTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get PickUpLoad for Trip
    }
    
    public async Task<Load?> GetDropOffLoadByTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get DropOffLoad for Trip
    }

    public async Task AddLoadAsync(Load load, CancellationToken cancellationToken)
    {
        context.Loads.Add(load);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RemoveLoadBayAsync(Load load, Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: load.Bay = null;
        // TODO: Bay.Loads.Remove(load);
        // TODO: Save
    }
    
    public async Task SetLoadBayAsync(Load load, Bay bay, CancellationToken cancellationToken)
    {
        var oldBay = await bayRepository.GetBayByLoadAsync(load, cancellationToken);
        if (oldBay != null)
        {
            await RemoveLoadBayAsync(load, oldBay, cancellationToken);
        }
        throw new NotImplementedException();
        // TODO: load.Bay = bay;
        // TODO: Bay.Loads.Add(load);
        // TODO: Save
    }
}