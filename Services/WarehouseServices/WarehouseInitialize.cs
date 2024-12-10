using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Services.ParkingSpotServices;

namespace Services.WarehouseServices;

public sealed class WarehouseInitialize(
    ILogger<WarehouseInitialize> logger,
    WarehouseService warehouseService,
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
            var warehouse = await warehouseService.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New Warehouse created: Warehouse={@Warehouse}", warehouse);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await InitializeObjectAsync(cancellationToken);
    }
}