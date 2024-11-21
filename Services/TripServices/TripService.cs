using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.AdminStaffServices;
using Services.BayServices;
using Services.ParkingSpotServices;
using Services.TruckServices;
using Settings;

namespace Services.TripServices;

public sealed class TripService(
    LoadService loadService,
    WorkRepository workRepository,
    ParkingSpotService parkingSpotService,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository,
    AdminStaffService adminStaffService,
    TripRepository tripRepository,
    HubRepository hubRepository,
    BayService bayService,
    BayRepository bayRepository,
    TruckRepository truckRepository,
    TruckService truckService,
    LocationService locationService,
    TruckCompanyRepository truckCompanyRepository)
{
    public async Task<Trip?> GetNewObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOff = await loadService.SelectUnclaimedDropOffAsync(truckCompany, cancellationToken);
        Load? pickUp = null;
        Hub? hub = null;

        if (dropOff == null)
        {
            pickUp = await loadService.SelectUnclaimedPickUpAsync(truckCompany, cancellationToken);
            if (pickUp == null) return null;
            // TODO: Log no Trip could be created
            hub = await hubRepository.GetAsync(pickUp, cancellationToken);
        }
        else
        {
            hub = await hubRepository.GetAsync(dropOff, cancellationToken);
            if (hub == null)
                throw new Exception("DropOff Load was not matched on a valid Hub.");

            pickUp = await loadService.SelectUnclaimedPickUpAsync(hub, truckCompany, cancellationToken);
        }

        var trip = new Trip
        {
            DropOff = dropOff,
            PickUp = pickUp,
            Hub = hub
        };
        await locationService.SetAsync(trip, truckCompany, cancellationToken);
        
        return trip;
    }
    
    public async Task<Trip> SelectTripAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trips = await (await tripRepository.GetAsync(truckCompany, cancellationToken))
            .ToListAsync(cancellationToken);

        if (trips.Count <= 0) 
            throw new Exception("There was no Trip assigned to this TruckCompany.");

        var trip = trips[ModelConfig.Random.Next(trips.Count)];
        return trip;
    }

    public async Task<Trip?> GetNextAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await tripRepository.GetCurrentAsync(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        await foreach (var trip in trips)
        {
            var work = await workRepository.GetAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            nextTrip = trip;
            earliestStart = work?.StartTime;
        }

        return nextTrip;
    }
    
    public async Task AlertFreeAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork == null)
        {
                await truckService.AlertClaimedAsync(truck, trip, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);
        }
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitParking })
        {
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await parkingSpotService.AlertClaimedAsync(parkingSpot, trip, cancellationToken);
                await locationService.SetAsync(trip, parkingSpot, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);

        }
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitCheckIn })
        {
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await adminStaffService.AlertClaimedAsync(adminStaff, trip, cancellationToken);
            await workRepository.AddAsync(trip, adminStaff, cancellationToken);
        }
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitBay })
        {
            var parkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
            if (parkingSpot != null)
            {
                await parkingSpotService.AlertUnclaimedAsync(parkingSpot, cancellationToken);
            }

            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await bayService.AlertClaimedAsync(bay, trip, cancellationToken);
                await locationService.SetAsync(trip, bay, cancellationToken);
            await workRepository.AddAsync(trip, bay, cancellationToken);
        }
    }
    
    public async Task AlertTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.TravelHub })
        {
            await workRepository.RemoveAsync(oldWork, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitParking, cancellationToken);
        }
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.CheckIn })
        {
            var adminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
            if (adminStaff == null)
                throw new Exception ("The CheckIn for this Trip has just completed but there was no AdminStaff assigned.");
            
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await adminStaffService.AlertUnclaimedAsync(adminStaff, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitBay, cancellationToken);
        }
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.Bay })
        {
            var bay = await bayRepository.GetAsync(trip, cancellationToken);
            if (bay == null)
                throw new Exception ("The Bay Work for this Trip has just completed but there was no Bay assigned.");
            
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await bayService.AlertUnclaimedAsync(bay, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.TravelHome, cancellationToken);
        }
    }
    
    public async Task AlertTravelHomeCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.TravelHome })
        {
            var truck = await truckRepository.GetAsync(trip, cancellationToken);
            if (truck == null)
                throw new Exception("The Trip has just completed but there was no Truck assigned.");
            
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await truckService.AlertUnclaimedAsync(truck, cancellationToken);
        }
    }
    
    public async Task TravelAsync(Trip trip, Truck truck, long xDestination, long yDestination, CancellationToken cancellationToken)
    {
        var xDiff = xDestination - trip.XLocation;
        var xTravel = xDiff > truck.Speed ? truck.Speed : xDiff;
        var newXLocation = trip.XLocation + xTravel;
        
        var yDiff = yDestination - trip.YLocation;
        var yTravel = yDiff > truck.Speed ? truck.Speed : yDiff;
        var newYLocation = trip.YLocation + yTravel;

        await locationService.SetAsync(trip, newXLocation, newYLocation, cancellationToken);
    }
    
    public async Task TravelAsync(Trip trip, Truck truck, Hub hub, CancellationToken cancellationToken)
    {
        await TravelAsync(trip, truck, hub.XLocation, hub.YLocation, cancellationToken);
    }
    
    public async Task TravelAsync(Trip trip, Truck truck, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await TravelAsync(trip, truck, truckCompany.XLocation, truckCompany.YLocation, cancellationToken);
    }

    public async Task TravelHubAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
            throw new Exception("The Trip was travelling home but there was no Truck assigned.");
        
        var hub = await hubRepository.GetAsync(trip, cancellationToken);
        if (hub == null)
            throw new Exception("The Trip was travelling to the hub but there was no Hub assigned.");
        
        await TravelAsync(trip, truck, hub, cancellationToken);
    }

    public async Task TravelHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
            throw new Exception("The Trip was travelling home but there was no Truck assigned.");
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
            throw new Exception("The Trip was travelling home but there was no TruckCompany assigned to its Truck.");
        
        await TravelAsync(trip, truck, truckCompany, cancellationToken);
    }


    public async Task<bool> IsAtHubAsync(Trip trip, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(trip, cancellationToken);
        return hub != null &&
               hub.XLocation == trip.XLocation &&
               hub.YLocation == trip.YLocation;
    }

    public async Task<bool> IsAtHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null) return false;
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        return truckCompany.XLocation == trip.XLocation &&
               truckCompany.YLocation == trip.YLocation;
    }
}
