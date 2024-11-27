using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services;

public class LoadService(
    ILogger<LoadService> logger,
    TruckCompanyService truckCompanyService,
    HubService hubService,
    LoadRepository loadRepository,
    ModelState modelState)
{
    public async Task<Load?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompanyStart = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompanyStart == null)
        {
            logger.LogError("No TruckCompany could be selected for the new Load start location.");

            return null;
        }
        
        var truckCompanyEnd = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompanyEnd == null)
        {
            logger.LogError("No TruckCompany could be selected for the new Load end location.");

            return null;
        }
        
        var hub = await hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new Load.");

            return null;
        }
        
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
            if (load == null)
            {
                logger.LogError("Could not construct a new Load...");
            
                return;
            }
            
            await loadRepository.AddAsync(load, cancellationToken);
            logger.LogInformation("New Load created: Load={@Load}", load);
        }
    }
    
    public async Task<Load?> SelectUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = await (loadRepository.GetUnclaimedDropOff(truckCompany))
            .ToListAsync(cancellationToken);

        if (dropOffs.Count <= 0)
        {
            logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Drop-Off Load assigned.",
                truckCompany);

            return null;
        }
        
        var dropOff = dropOffs[modelState.Random(dropOffs.Count)];
        return dropOff;
    }
    
    public async Task<Load?> SelectUnclaimedPickUpAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (loadRepository.GetUnclaimedPickUp(truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0)
        {
            logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned.",
                truckCompany);

            return null;
        }
        
        var pickUp = pickUps[modelState.Random(pickUps.Count)];
        return pickUp;
    }

    public async Task<Load?> SelectUnclaimedPickUpAsync(Hub hub, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (loadRepository.GetUnclaimedPickUp(hub, truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0)
        {
            logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned for Hub \n({@Hub})",
                truckCompany,
                hub);

            return null;
        }
        
        var pickUp = pickUps[modelState.Random(pickUps.Count)];
        return pickUp;
    }
}