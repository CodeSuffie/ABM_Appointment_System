using Database.Models;
using Repositories;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    TripRepository tripRepository,
    HubRepository hubRepository,
    WorkRepository workRepository)
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

    public async Task AlertClaimed(ParkingSpot parkingSpot, Trip trip, CancellationToken cancellationToken)
    {
        await tripRepository.SetTripParkingSpotAsync(trip, parkingSpot, cancellationToken);
        await workRepository.AddWorkAsync(trip, WorkType.WaitCheckIn, cancellationToken);
    }

    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldTrip = await tripRepository.GetTripByParkingSpotAsync(parkingSpot, cancellationToken);
        if (oldTrip != null)
        {
            await tripRepository.RemoveTripParkingSpotAsync(oldTrip, parkingSpot, cancellationToken);
        }

        var hub = await hubRepository.GetHubByParkingSpotAsync(parkingSpot, cancellationToken);
        if (hub == null) return;
        
        var newTrip = await tripRepository.GetNextTripByHubByWorkTypeAsync(hub, WorkType.WaitParking, cancellationToken);
        if (newTrip != null)
        {
            await AlertClaimed(parkingSpot, newTrip, cancellationToken);
        }
    }
}