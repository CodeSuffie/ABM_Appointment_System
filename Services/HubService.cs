using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services;

public sealed class HubService(
    ILogger<HubService> logger,
    HubRepository hubRepository,
    ModelState modelState)
{
    public async Task<Hub?> SelectHubAsync(CancellationToken cancellationToken)
    {
        var hubs = await (hubRepository.Get())
            .ToListAsync(cancellationToken);
            
        if (hubs.Count <= 0)
        {
            logger.LogError("Model did not have a Hub assigned.");

            return null;
        }
            
        var hub = hubs[modelState.Random(hubs.Count)];
        return hub;
    }
}
