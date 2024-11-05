using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            Hub = hub
        };
        
        // TODO: Add Location
        
        hub.Bays.Add(bay);
    }

    public async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayCount; i++)
        {
            await InitializeObjectAsync(hub, cancellationToken);
        }
    }
}
