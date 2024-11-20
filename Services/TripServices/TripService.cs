using Database.Models;
using Repositories;
using Services.ParkingSpotServices;

namespace Services.TripServices;

public sealed class TripService(
    LoadService loadService,
    WorkRepository workRepository,
    ParkingSpotService parkingSpotService,
    ParkingSpotRepository parkingSpotRepository,
    TripRepository tripRepository,
    HubRepository hubRepository) 
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
            var hub = await hubRepository.GetHubByLoadAsync(dropOff, cancellationToken);
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
    
    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await workRepository.AddWorkAsync(trip, adminStaff, cancellationToken);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var parkingSpot = await parkingSpotRepository.GetParkingSpotByTripAsync(trip, cancellationToken);
        if (parkingSpot == null) return;
        
        await parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
        
        await tripRepository.SetTripBayAsync(trip, bay, cancellationToken);
        await workRepository.AddWorkAsync(trip, bay, cancellationToken);
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