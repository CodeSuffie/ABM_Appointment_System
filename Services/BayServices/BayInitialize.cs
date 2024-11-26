using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.BayServices;

public sealed class BayInitialize(
    ILogger<BayInitialize> logger,
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
            var bay = bayService.GetNewObject(hub, cancellationToken);
            
            logger.LogDebug("Setting location for this Bay ({@Bay})...",
                bay);
            await locationService.InitializeObjectAsync(bay, cancellationToken);

            logger.LogDebug("Setting Bay ({@Bay}) to its Hub ({@Hub})...",
                bay,
                hub);
            await bayRepository.SetAsync(bay, hub, cancellationToken);
            logger.LogInformation("New Bay created: Bay={@Bay}", bay);
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