using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class TruckInitializer(
    ILogger<TruckInitializer> logger,
    TruckFactory truckFactory,
    ModelState modelState)  : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truck = await truckFactory.GetNewObjectAsync(cancellationToken);
        if (truck == null)
        {
            logger.LogError("Could not construct a new Truck...");
            
            return;
        }
        
        logger.LogInformation("New Truck created: Truck={@Truck}", truck);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.TruckCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}