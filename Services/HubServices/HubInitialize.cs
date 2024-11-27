using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.HubServices;

public sealed class HubInitialize(
    ILogger<HubInitialize> logger,
    HubService hubService,
    ModelState modelState) : IPriorityInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.GetNewObjectAsync(cancellationToken);
        logger.LogInformation("New Hub created: Hub={@Hub}", hub);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.HubLocations.GetLength(0); i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}