using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.ModelServices;
using Settings;

namespace Services.HubServices;

public sealed class HubService(
    HubRepository hubRepository,
    ModelState modelState)
{
    public Task<Hub> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub
        {
            XSize = modelState.AgentConfig.HubXSize,
            YSize = modelState.AgentConfig.HubYSize,
            OperatingChance = modelState.AgentConfig.HubAverageOperatingDays,
            AverageOperatingHourLength = modelState.AgentConfig.OperatingHourAverageLength
        };

        return Task.FromResult(hub);
    }

    public async Task<Hub> SelectHubAsync(CancellationToken cancellationToken)
    {
        var hubs = await (await hubRepository.GetAsync(cancellationToken))
            .ToListAsync(cancellationToken);
            
        if (hubs.Count <= 0) throw new Exception("There was no Hub to select.");
            
        var hub = hubs[modelState.Random(hubs.Count)];
        return hub;
    }
}
