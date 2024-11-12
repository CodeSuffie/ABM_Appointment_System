using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Services.TripServices;

public sealed class TripService(
    ModelDbContext context,
    LoadService loadService) 
{
    public async Task<Work?> GetWorkForTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Trip?> GetNewObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOff = await loadService.SelectUnclaimedDropOffAsync(truckCompany, cancellationToken);
        Load? pickUp = null;

        if (dropOff == null)
        {
            pickUp = await loadService.SelectUnclaimedPickUpAsync(cancellationToken);
        }
        else
        {
            var hub = await context.Hubs
                .FirstOrDefaultAsync(x => x.Id == dropOff.HubId, cancellationToken);
            if (hub == null) throw new Exception("DropOff Load was not matched on a valid Hub.");
            
            pickUp = await loadService.SelectUnclaimedPickUpAsync(hub, cancellationToken);
        }
        
        var trip = new Trip
        {
            DropOff = dropOff,
            PickUp = pickUp,
            CheckedIn = false
        };

        return trip;
    }
}