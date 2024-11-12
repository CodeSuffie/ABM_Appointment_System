using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.HubServices;

public sealed class HubStepper(ModelDbContext context) : IStepperService<Hub>
{
    public async Task StepAsync(Hub hub, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create new Staff Shifts
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            await StepAsync(hub, cancellationToken);
        }
    }
}