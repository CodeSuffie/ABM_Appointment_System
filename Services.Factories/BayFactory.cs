using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class BayFactory(
    ILogger<BayFactory> logger,
    BayRepository bayRepository,
    LocationFactory locationFactory,
    ModelState modelState) : IFactoryService<Bay>
{
    public async Task<Bay?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            XSize = 1,
            YSize = 1,
            Capacity = modelState.AgentConfig.BayAverageCapacity,
            BayStatus = BayStatus.Closed
        };

        await bayRepository.AddAsync(bay, cancellationToken);

        return bay;
    }
    
    public async Task<Bay?> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = await GetNewObjectAsync(cancellationToken);
        if (bay == null)
        {
            logger.LogError("Bay could not be created.");

            return null;
        }

        logger.LogDebug("Setting Bay \n({@Bay})\n to its Hub \n({@Hub})", bay, hub);
        await bayRepository.SetAsync(bay, hub, cancellationToken);
        
        logger.LogDebug("Setting location for this Bay \n({@Bay})", bay);
        await locationFactory.InitializeObjectAsync(bay, cancellationToken);

        return bay;
    }
    
    
    public async Task<Bay?> SelectBayAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bays = await (bayRepository.Get(hub))
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0)
        {
            logger.LogError("Hub \n({@Hub})\n did not have a Bay assigned.", hub);

            return null;
        }

        var bay = bays[modelState.Random(bays.Count)];
        return bay;
    }
}