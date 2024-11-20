using Database.Models;
using Repositories;
using Services.TripServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    TripRepository tripRepository,
    TripService tripService,
    HubRepository hubRepository)
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

    public async Task AlertClaimedAsync(ParkingSpot parkingSpot, Trip trip, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, parkingSpot, cancellationToken);
    }

    public async Task AlertUnclaimedAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(parkingSpot, cancellationToken);
        if (trip == null)
            throw new Exception("This ParkingSpot was just told to be unclaimed but no Trip is assigned");

        await tripRepository.UnsetAsync(trip, parkingSpot, cancellationToken);
    }
    
    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null) 
            throw new Exception("This ParkingSpot was just told to be free but no Hub is assigned");
        
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitParking, cancellationToken);
        if (trip == null) return;    // TODO: Log no waiting Trips
        
        await tripService.AlertFreeAsync(trip, parkingSpot, cancellationToken);
    }
}
