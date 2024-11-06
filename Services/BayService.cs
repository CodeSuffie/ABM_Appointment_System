using Database;
using Database.Models;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayService(
    ModelDbContext context,
    LocationService locationService
    ) : IInitializationService, IStepperService<Bay>
{
    public async Task<Bay> GetNewAgentAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            Hub = hub,
        };

        var bayCount = context.Bays
            .Count(x => x.HubId == hub.Id);
        
        await locationService.InitializeObjectAsync(bay, bayCount, cancellationToken);

        return bay;
    }

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var bay = await GetNewAgentAsync(hub, cancellationToken);
            hub.Bays.Add(bay);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public Task ExecuteStepAsync(Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var bays = context.Bays
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bay in bays)
        {
            await ExecuteStepAsync(bay, cancellationToken);
        }
    }
}
