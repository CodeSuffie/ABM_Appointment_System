using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Settings;

namespace Services.HubServices;

public sealed class HubService(HubRepository hubRepository)
{
    public async Task<Hub> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub
        {
            XSize = AgentConfig.HubXSize,
            YSize = AgentConfig.HubYSize,
            OperatingChance = AgentConfig.HubAverageOperatingDays,
            AverageOperatingHourLength = AgentConfig.OperatingHourAverageLength
        };

        return hub;
    }

    public async Task<Hub> SelectHubAsync(CancellationToken cancellationToken)
    {
        var hubs = await (await hubRepository.GetAsync(cancellationToken))
            .ToListAsync(cancellationToken);
            
        if (hubs.Count <= 0) throw new Exception("There was no Hub to select.");
            
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        return hub;
    }
}
