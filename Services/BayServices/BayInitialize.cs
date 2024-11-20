using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.BayServices;

public sealed class BayInitialize(
    BayService bayService,
    LocationService locationService,
    HubRepository hubRepository,
    BayRepository bayRepository) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = (await hubRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var bay = await bayService.GetNewObjectAsync(hub, cancellationToken);
        
            await locationService.InitializeObjectAsync(bay, cancellationToken);

            await bayRepository.SetAsync(bay, hub, cancellationToken);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}