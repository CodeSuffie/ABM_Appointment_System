using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class WarehouseFactory(
    ILogger<WarehouseFactory> logger,
    LocationFactory locationFactory,
    WarehouseRepository warehouseRepository,
    ModelState modelState) : IFactoryService<Warehouse>
{
    public async Task<Warehouse?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            XSize = modelState.AgentConfig.WarehouseXSize,
            YSize = modelState.AgentConfig.WarehouseYSize,
            Capacity = modelState.AgentConfig.WarehouseAverageCapacity,
        };
        
        await warehouseRepository.AddAsync(warehouse, cancellationToken);

        return warehouse;
    }
    
    public async Task<Warehouse?> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var warehouse = await GetNewObjectAsync(cancellationToken);
        if (warehouse == null)
        {
            logger.LogError("Warehouse could not be created.");

            return null;
        }

        logger.LogDebug("Setting Warehouse \n({@Warehouse})\n to its Hub \n({@Hub})", warehouse, hub);
        await warehouseRepository.SetAsync(warehouse, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Warehouse \n({@Warehouse})", warehouse);
        locationFactory.InitializeObject(warehouse, cancellationToken);

        return warehouse;
    }
}