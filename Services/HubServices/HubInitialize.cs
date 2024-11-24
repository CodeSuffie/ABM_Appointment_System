using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.HubServices;

public sealed class HubInitialize(
    HubService hubService,
    OperatingHourService operatingHourService, 
    LocationService locationService,
    HubRepository hubRepository,
    ModelState modelState) : IPriorityInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.GetNewObjectAsync(cancellationToken);
        
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        await operatingHourService.GetNewObjectsAsync(hub, cancellationToken);

        await hubRepository.AddAsync(hub, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.HubCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}