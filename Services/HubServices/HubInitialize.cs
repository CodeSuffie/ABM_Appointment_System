using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.HubServices;

public sealed class HubInitialize(
    ILogger<HubInitialize> logger,
    HubService hubService,
    OperatingHourService operatingHourService, 
    LocationService locationService,
    HubRepository hubRepository,
    ModelState modelState) : IPriorityInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = hubService.GetNewObject();
        
        logger.LogDebug("Setting location for this Hub ({@Hub})...",
            hub);
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        
        logger.LogDebug("Setting OperatingHours for this Hub ({@Hub})...",
            hub);
        operatingHourService.GetNewObjects(hub);

        await hubRepository.AddAsync(hub, cancellationToken);
        logger.LogInformation("New Hub created: Hub={@Hub}", hub);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.HubLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}