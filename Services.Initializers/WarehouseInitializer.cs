using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Initializers;

public sealed class WarehouseInitializer(
    ILogger<WarehouseInitializer> logger,
    WarehouseService warehouseService,
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
            var warehouse = await warehouseService.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New Warehouse created: Warehouse={@Warehouse}", warehouse);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await InitializeObjectAsync(cancellationToken);
    }
}