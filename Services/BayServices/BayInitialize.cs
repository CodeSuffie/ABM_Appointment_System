using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.BayServices;

public sealed class BayInitialize(
    BayService bayService,
    LocationService locationService,
    HubRepository hubRepository,
    BayRepository bayRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
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
        for (var i = 0; i < modelState.AgentConfig.BayLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}