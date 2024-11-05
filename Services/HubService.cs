using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class HubService(
    ModelDbContext context, 
    OperatingHourService operatingHourService, 
    LocationService locationService,
    ParkingSpotService parkingSpotService,
    BayService bayService
    ) : IAgentService<Hub>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub();
        
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        
        await operatingHourService.InitializeObjectsAsync(hub, cancellationToken);
        await parkingSpotService.InitializeObjectsAsync(hub, cancellationToken);
        await bayService.InitializeObjectsAsync(hub, cancellationToken);
        
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
