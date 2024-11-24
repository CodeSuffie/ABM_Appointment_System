using Database.Models;
using Repositories;
using Services.HubServices;
using Services.TripServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotService(
    TripService tripService,
    HubLogger hubLogger,
    HubRepository hubRepository)
{
    public Task<ParkingSpot> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        return Task.FromResult(parkingSpot);
    }
    
    public async Task AlertFreeAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        // TODO: PerkingSpot Stepper, if no Truck at parking spot, get another
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null) 
            throw new Exception("This ParkingSpot was just told to be free but no Hub is assigned");
        
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitParking, cancellationToken);
        if (trip == null)
        {
            await hubLogger.LogAsync(hub, parkingSpot, LogType.Info, "No Trips waiting for a Parking Spot.", cancellationToken);
            return;
        }
        
        await tripService.AlertFreeAsync(trip, parkingSpot, cancellationToken);
    }
}
