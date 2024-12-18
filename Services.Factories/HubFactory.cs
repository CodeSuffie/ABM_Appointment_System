using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class HubFactory(
    ILogger<HubFactory> logger,
    HubRepository hubRepository,
    LocationFactory locationFactory,
    OperatingHourFactory operatingHourFactory, 
    ModelState modelState) : IFactoryService<Hub>
{
    public async Task<Hub?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub
        {
            XSize = modelState.AgentConfig.HubXSize,
            YSize = modelState.AgentConfig.HubYSize,
            WorkChance = modelState.AgentConfig.HubAverageWorkDays,
            AverageShiftLength = modelState.AgentConfig.OperatingHourAverageLength
        };

        await hubRepository.AddAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Hub \n({@Hub})",
            hub);
        await locationFactory.InitializeObjectAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting OperatingHours for this Hub \n({@Hub})",
            hub);
        await operatingHourFactory.GetNewObjectsAsync(hub, cancellationToken);

        return hub;
    }
    
    public async Task<Hub?> SelectHubAsync(CancellationToken cancellationToken)
    {
        var hubs = await (hubRepository.Get())
            .ToListAsync(cancellationToken);
            
        if (hubs.Count <= 0)
        {
            logger.LogError("Model did not have a Hub assigned.");

            return null;
        }
            
        var hub = hubs[modelState.Random(hubs.Count)];
        return hub;
    }
}