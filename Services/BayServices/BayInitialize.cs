using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.BayServices;

public sealed class BayInitialize(
    ILogger<BayInitialize> logger,
    BayService bayService,
    HubRepository hubRepository,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Normal;
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var bay = await bayService.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New Bay created: Bay={@Bay}", bay);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.BayLocations.GetLength(0); i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}