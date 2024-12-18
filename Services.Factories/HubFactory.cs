using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class HubFactory(
    ILogger<HubFactory> logger,
    HubRepository hubRepository,
    LocationService locationService,
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
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting OperatingHours for this Hub \n({@Hub})",
            hub);
        await operatingHourFactory.GetNewObjectsAsync(hub, cancellationToken);

        return hub;
    }
}