using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class HubService(ModelDbContext context) : IAgentService<Hub>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub();
        
        // TODO: Add Location
        
        await OperatingHourService.InitializeObjectsAsync(hub, cancellationToken);
        await ParkingSpotService.InitializeObjectsAsync(hub, cancellationToken);
        
        context.Hubs.Add(hub);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.HubCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Hub hub, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var hubs = await context.Hubs.ToListAsync(cancellationToken);
        foreach (var hub in hubs)
        {
            await ExecuteStepAsync(hub, cancellationToken);
        }
    }
}
