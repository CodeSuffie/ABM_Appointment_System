using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.BayServices;
using Services.ParkingSpotServices;

namespace Services.TripServices;

public sealed class TripService(
    ModelDbContext context,
    LoadService loadService,
    WorkService workService,
    ParkingSpotService parkingSpotService,
    BayService bayService) 
{
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
        };

        return trip;
    }
    
    // TODO: Repository
    public async Task<Work?> GetWorkForTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    // TODO: Repository
    private async Task<ParkingSpot?> GetParkingSpotForTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get Parking Spot for Trip
    }

    // TODO: Repository
    public async Task<Load?> GetPickUpLoadForTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get PickUpLoad for Trip
    }

    // TODO: Repository
    public async Task<Load?> GetDropOffLoadForTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Get DropOffLoad for Trip
    }
    
    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await workService.AddWorkAsync(trip, adminStaff, cancellationToken);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var parkingSpot = await GetParkingSpotForTripAsync(trip, cancellationToken);
        if (parkingSpot == null) return;
        
        await parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
        
        await bayService.AddTripAsync(bay, trip, cancellationToken);
        await workService.AddWorkAsync(trip, bay, cancellationToken);
    }

    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Remove My Work
        // TODO: Add Work for BayWait
    }

    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Remove My Work
        // TODO: Add Work for TravelHome
    }

    
}