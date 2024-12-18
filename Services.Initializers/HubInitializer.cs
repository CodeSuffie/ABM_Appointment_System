using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class HubInitializer(
    ILogger<HubInitializer> logger,
    HubFactory hubFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.High;
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubFactory.GetNewObjectAsync(cancellationToken);
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