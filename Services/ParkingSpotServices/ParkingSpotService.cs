using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.HubServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    ModelDbContext context,
    HubService hubService,
    WorkService workService)
{
    public async Task<ParkingSpot> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        return parkingSpot;
    }

    // TODO: Repository
    public async Task<Hub?> GetHubForParkingSpotAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == parkingSpot.HubId, cancellationToken);

        return hub;
    }
    
    // TODO: Repository
    public async Task<Trip?> GetTripForParkingSpotAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(x => x.Id == parkingSpot.TripId, cancellationToken);

        return trip;
    }
    
    // TODO: Repository
    public async Task RemoveTripAsync(ParkingSpot parkingSpot, Trip trip, CancellationToken cancellationToken)
    {
        trip.ParkingSpot = null;
        parkingSpot.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    // TODO: Repository
    public async Task AddTripAsync(ParkingSpot parkingSpot, Trip trip, CancellationToken cancellationToken)
    {
        // TODO: If already an active ParkingSpot or Bay, throw Exception or Log
        
        trip.ParkingSpot = parkingSpot;
        parkingSpot.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AlertClaimed(ParkingSpot parkingSpot, Trip trip, CancellationToken cancellationToken)
    {
        await AddTripAsync(parkingSpot, trip, cancellationToken);
        await workService.AddWorkAsync(trip, WorkType.WaitCheckIn, cancellationToken);
    }

    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldTrip = await GetTripForParkingSpotAsync(parkingSpot, cancellationToken);
        if (oldTrip != null)
        {
            await RemoveTripAsync(parkingSpot, oldTrip, cancellationToken);
        }

        var hub = await GetHubForParkingSpotAsync(parkingSpot, cancellationToken);
        if (hub == null) return;
        
        var newTrip = await hubService.GetNextParkingTripAsync(hub, cancellationToken);
        if (newTrip != null)
        {
            await AlertClaimed(parkingSpot, newTrip, cancellationToken);
        }
    }
}