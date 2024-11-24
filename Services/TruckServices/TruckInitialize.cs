using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.TruckServices;

public sealed class TruckInitialize(
    TruckService truckService,
    TruckRepository truckRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truck = await truckService.GetNewObjectAsync(cancellationToken);

        await truckRepository.AddAsync(truck, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.TruckCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}