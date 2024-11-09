using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.BayServices;

public sealed class BayStepper(ModelDbContext context) : IStepperService<Bay>
{
    public Task StepAsync(Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bays = context.Bays
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bay in bays)
        {
            await StepAsync(bay, cancellationToken);
        }
    }
}