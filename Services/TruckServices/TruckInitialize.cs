using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TruckServices;

public sealed class TruckInitialize(
    ILogger<TruckInitialize> logger,
    TruckService truckService,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truck = await truckService.GetNewObjectAsync(cancellationToken);
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