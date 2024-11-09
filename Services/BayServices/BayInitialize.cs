using Database;
using Services.Abstractions;
using Settings;

namespace Services.BayServices;

public sealed class BayInitialize(
    ModelDbContext context,
    BayService bayService,
    LocationService locationService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var bay = await bayService.GetNewObjectAsync(hub, cancellationToken);
        
            await locationService.InitializeObjectAsync(bay, cancellationToken);
            
            hub.Bays.Add(bay);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}