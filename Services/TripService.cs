using Database;
using Database.Models;
using Services.Abstractions;

namespace Services;

public sealed class TripService(ModelDbContext context) : IStepperService<Trip>
{
    public async Task ExecuteStepAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            await ExecuteStepAsync(trip, cancellationToken);
        }
    }
}