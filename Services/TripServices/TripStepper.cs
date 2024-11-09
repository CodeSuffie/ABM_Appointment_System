using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.TripServices;

public sealed class TripStepper(ModelDbContext context) : IStepperService<Trip>
{
    public async Task StepAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            await StepAsync(trip, cancellationToken);
        }
    }
}