using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.WarehouseServices;

public sealed class WarehouseService(
    ILogger<WarehouseService> logger,
    LocationService locationService,
    WarehouseRepository warehouseRepository,
    ModelState modelState)
{
    public async Task<Warehouse> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            XSize = modelState.AgentConfig.WarehouseXSize,
            YSize = modelState.AgentConfig.WarehouseYSize,
            Hub = hub,
        };
        
        await warehouseRepository.AddAsync(warehouse, cancellationToken);

        logger.LogDebug("Setting Warehouse \n({@Warehouse})\n to its Hub \n({@Hub})",
            warehouse,
            hub);
        await warehouseRepository.SetAsync(warehouse, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Warehouse \n({@Warehouse})",
            warehouse);
        locationService.InitializeObject(warehouse, cancellationToken);

        return warehouse;
    }
}