using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class WarehouseInitializer(
    ILogger<WarehouseInitializer> logger,
    WarehouseFactory warehouseFactory,
    HubRepository hubRepository,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Normal;

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var warehouse = await warehouseFactory.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New Warehouse created: Warehouse={@Warehouse}", warehouse);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await InitializeObjectAsync(cancellationToken);
    }
}