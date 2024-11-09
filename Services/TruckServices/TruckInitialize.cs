using Database;
using Services.Abstractions;
using Settings;

namespace Services.TruckServices;

public sealed class TruckInitialize(
    ModelDbContext context,
    TruckService truckService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truck = await truckService.GetNewObjectAsync(cancellationToken);
        
        context.Trucks
            .Add(truck);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}