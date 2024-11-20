using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.TruckServices;

public sealed class TruckInitialize(
    TruckService truckService,
    TruckRepository truckRepository) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truck = await truckService.GetNewObjectAsync(cancellationToken);

        await truckRepository.AddAsync(truck, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}