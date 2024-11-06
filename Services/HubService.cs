using Database;
using Database.Models;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class HubService(
    ModelDbContext context, 
    OperatingHourService operatingHourService, 
    LocationService locationService
    ) : IInitializationService, IStepperService<Hub>
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub();
        
        await locationService.InitializeObjectAsync(hub, cancellationToken);
        await operatingHourService.InitializeObjectsAsync(hub, cancellationToken);
        
        context.Hubs
            .Add(hub);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.HubCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Hub hub, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            await ExecuteStepAsync(hub, cancellationToken);
        }
    }
}
