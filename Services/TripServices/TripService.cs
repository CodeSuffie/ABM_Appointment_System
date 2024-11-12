using Database;
using Database.Models;

namespace Services.TripServices;

public sealed class TripService(
    ModelDbContext context,
    LoadService loadService) 
{
    public async Task<Trip> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var dropOff = await loadService.SelectDropOffAsync(cancellationToken);
        var pickUp = await loadService.SelectPickUpAsync(cancellationToken);
        
        var trip = new Trip
        {
            DropOff = dropOff,
            PickUp = pickUp
        };

        return trip;
    }
}