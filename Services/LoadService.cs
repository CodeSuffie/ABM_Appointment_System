using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.HubServices;
using Services.TruckCompanyServices;
using Settings;

namespace Services;

public class LoadService(
    ModelDbContext context,
    TruckCompanyService truckCompanyService,
    HubService hubService)
{
    public async Task<Load> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompanyStart = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        var truckCompanyEnd = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        
        var hub = await hubService.SelectHubAsync(cancellationToken);
        
        var load = new Load
        {
            LoadType = LoadType.DropOff,
            TruckCompanyStart = truckCompanyStart,
            TruckCompanyEnd = truckCompanyEnd,
            Hub = hub
        };

        return load;
    }
    
    public async Task AddNewLoads(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var load = await GetNewObjectAsync(cancellationToken);
            context.Loads.Add(load);
        }
    }
    
    public async Task<Load?> SelectUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = await context.Loads
            .Where(x => x.DropOffTrip == null)
            .Where(x => x.TruckCompanyStartId == truckCompany.Id)
            .ToListAsync(cancellationToken);

        if (dropOffs.Count <= 0) return null;
        
        var dropOff = dropOffs[ModelConfig.Random.Next(dropOffs.Count)];
        return dropOff;
    }
    
    public async Task<Load?> SelectUnclaimedPickUpAsync(CancellationToken cancellationToken)
    {
        var pickUps = await context.Loads
            .Where(x => x.PickUpTrip == null)
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0) return null;
        
        var pickUp = pickUps[ModelConfig.Random.Next(pickUps.Count)];
        return pickUp;
    }

    public async Task<Load?> SelectUnclaimedPickUpAsync(Hub hub, CancellationToken cancellationToken)
    {
        var pickUps = await context.Loads
            .Where(x => x.PickUpTrip == null)
            .Where(x => x.HubId == hub.Id)
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0) return null;
        
        var pickUp = pickUps[ModelConfig.Random.Next(pickUps.Count)];
        return pickUp;
    }
}