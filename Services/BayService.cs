using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(Hub hub, int i, CancellationToken cancellationToken)
    {
        var location = new Location
        {
            LocationType = LocationType.BaySpot,
            XLocation = AgentConfig.BayLocations[i, 0],
            YLocation = AgentConfig.BayLocations[i, 1],
        };
        
        var bay = new Bay
        {
            Hub = hub,
            Location = location
        };
        
        hub.Bays.Add(bay);
    }

    public async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayLocations.Length; i++)
        {
            await InitializeObjectAsync(hub, i, cancellationToken);
        }
    }
}
