using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.HubServices;

public sealed class HubService(
    ILogger<HubService> logger,
    HubRepository hubRepository,
    OperatingHourService operatingHourService, 
    LocationService locationService,
    ModelState modelState)
{
    public async Task<Hub> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub
        {
            XSize = modelState.AgentConfig.HubXSize,
            YSize = modelState.AgentConfig.HubYSize,
            OperatingChance = modelState.AgentConfig.HubAverageOperatingDays,
            AverageOperatingHourLength = modelState.AgentConfig.OperatingHourAverageLength
        };

        await hubRepository.AddAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Hub ({@Hub})...",
            hub);
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting OperatingHours for this Hub ({@Hub})...",
            hub);
        operatingHourService.GetNewObjects(hub);

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
