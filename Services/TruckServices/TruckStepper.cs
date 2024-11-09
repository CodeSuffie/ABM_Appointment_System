using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.TruckServices;

public sealed class TruckStepper(ModelDbContext context): IStepperService<Truck>
{
    public async Task StepAsync(Truck truck, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trucks = context.Trucks
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            await StepAsync(truck, cancellationToken);
        }
    }
}