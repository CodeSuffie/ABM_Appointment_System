using Database;
using Services.Abstractions;
using Settings;

namespace Services.HubServices;

public sealed class HubInitialize(
    ModelDbContext context,  
    HubService hubService,
    OperatingHourService operatingHourService, 
    LocationService locationService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.GetNewObjectAsync(cancellationToken);
        
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        await operatingHourService.GetNewObjectsAsync(hub, cancellationToken);
        
        context.Hubs
            .Add(hub);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.HubCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}