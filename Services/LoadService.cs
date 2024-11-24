using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;
using Settings;

namespace Services;

public class LoadService(
    TruckCompanyService truckCompanyService,
    HubService hubService,
    LoadRepository loadRepository,
    ModelState modelState)
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
    
    public async Task AddNewLoadsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var load = await GetNewObjectAsync(cancellationToken);
            await loadRepository.AddAsync(load, cancellationToken);
        }
    }
    
    public async Task<Load?> SelectUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = await (loadRepository.GetUnclaimedDropOff(truckCompany))
            .ToListAsync(cancellationToken);

        if (dropOffs.Count <= 0) return null;
        
        var dropOff = dropOffs[modelState.Random(dropOffs.Count)];
        return dropOff;
    }
    
    public async Task<Load?> SelectUnclaimedPickUpAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (loadRepository.GetUnclaimedPickUp(truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0) return null;
        
        var pickUp = pickUps[modelState.Random(pickUps.Count)];
        return pickUp;
    }

    public async Task<Load?> SelectUnclaimedPickUpAsync(Hub hub, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (loadRepository.GetUnclaimedPickUp(hub, truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0) return null;
        
        var pickUp = pickUps[modelState.Random(pickUps.Count)];
        return pickUp;
    }
}