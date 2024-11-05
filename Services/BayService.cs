using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayService(ModelDbContext context) : IAgentService<Bay>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];

        var bay = new Bay
        {
            Hub = hub
        };
        
        // TODO: Add Location
        // TODO: Add AvailableLoads
        
        context.Bays.Add(bay);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Bay bay, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var bays = await context.Bays.ToListAsync(cancellationToken);
        foreach (var bay in bays)
        {
            await ExecuteStepAsync(bay, cancellationToken);
        }
    }
}
