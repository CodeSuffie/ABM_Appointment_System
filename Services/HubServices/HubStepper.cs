using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.HubServices;

public sealed class HubStepper(ModelDbContext context) : IStepperService<Hub>
{
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