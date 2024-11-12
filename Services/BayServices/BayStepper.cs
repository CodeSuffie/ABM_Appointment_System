using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.BayServices;

public sealed class BayStepper(ModelDbContext context) : IStepperService<Bay>
{
    public Task StepAsync(Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: If not opened, wait
        // TODO: If free, and no waiting Truck, wait
        // TODO: Otherwise alert next Truck I am free
        // TODO: If not free, continue handling Trip
        // TODO: If Load was dropped off, increase Bay Inventory, handle Load change to PickUp
        // TODO: If Load was picked up, decrease Bay Inventory
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