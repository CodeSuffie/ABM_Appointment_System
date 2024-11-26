using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.TripServices;

public sealed class TripService(
    ILogger<TripService> logger,
    LoadService loadService,
    WorkRepository workRepository,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository,
    TripRepository tripRepository,
    HubRepository hubRepository,
    BayRepository bayRepository,
    TruckRepository truckRepository,
    LocationService locationService,
    TruckCompanyRepository truckCompanyRepository,
    WorkService workService,
    LoadRepository loadRepository,
    ModelState modelState)
{
    public async Task<Trip?> GetNewObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        while (true)
        {
            var dropOff = await loadService.SelectUnclaimedDropOffAsync(truckCompany, cancellationToken);
            Load? pickUp;
            Hub? dropOffHub = null;
            Hub? pickUpHub = null;

            if (dropOff == null)
            {
                logger.LogInformation("TruckCompany ({@TruckCompany}) did not have an unclaimed Drop-Off Load to be assigned to this new Trip.",
                    truckCompany);

                pickUp = await loadService.SelectUnclaimedPickUpAsync(truckCompany, cancellationToken);
                if (pickUp == null)
                {
                    logger.LogInformation("TruckCompany ({@TruckCompany}) did not have any Load to be assigned to this new Trip.",
                        truckCompany);

                    return null;
                }

                pickUpHub = await hubRepository.GetAsync(pickUp, cancellationToken);
            }
            else
            {
                dropOffHub = await hubRepository.GetAsync(dropOff, cancellationToken);
                if (dropOffHub == null)
                {
                    logger.LogError("Drop-Off Load ({@Load}) for this TruckCompany ({@TruckCompany}) did not have a Hub assigned.",
                        dropOff,
                        truckCompany);

                    logger.LogDebug("Removing invalid Drop-Off Load ({@Load}) for this TruckCompany ({@TruckCompany}).",
                        dropOff,
                        truckCompany);
                    await loadRepository.RemoveAsync(dropOff, cancellationToken);

                    continue;
                }

                pickUp = await loadService.SelectUnclaimedPickUpAsync(dropOffHub, truckCompany, cancellationToken);
            }

            var hub = dropOffHub ?? pickUpHub;
            if (hub == null)
            {
                logger.LogError("Pick-Up Load ({@Load}) for this TruckCompany ({@TruckCompany}) did not have a Hub assigned.",
                    pickUp,
                    truckCompany);

                if (pickUp != null)
                {
                    logger.LogDebug("Removing invalid Load Pick-Up ({@Load}) for this TruckCompany ({@TruckCompany}).",
                        dropOff,
                        truckCompany);
                    await loadRepository.RemoveAsync(pickUp, cancellationToken);
                }
                
                continue;
            }

            var trip = new Trip
            {
                DropOff = dropOff,
                PickUp = pickUp,
                Hub = hub
            };

            logger.LogDebug("Setting TruckCompany ({@TruckCompany}) location to this Trip ({@Trip})...",
                truckCompany,
                trip);
            await locationService.SetAsync(trip, truckCompany, cancellationToken);

            return trip;
        }
    }

    public async Task AddNewObjectsAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trip = await GetNewObjectAsync(truckCompany, cancellationToken);
        while (trip != null)
        {
            await tripRepository.AddAsync(trip, cancellationToken);
            logger.LogInformation("New Trip created for this TruckCompany ({@TruckCOmpany}): Trip={@Trip}",
                truckCompany,
                trip);
            
            trip = await GetNewObjectAsync(truckCompany, cancellationToken);
        }
        
        logger.LogInformation("TruckCompany ({@TruckCompany}) could not construct any more new Trips...",
            truckCompany);
    }
    
    public async Task<Trip?> GetNextAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trips = await (tripRepository.Get(truckCompany))
            .ToListAsync(cancellationToken);

        if (trips.Count <= 0)
        {
            logger.LogInformation("TruckCompany ({@TruckCompany}) did not have a Trip assigned.",
                truckCompany);

            return null;
        }

        var trip = trips[modelState.Random(trips.Count)];
        return trip;
    }

    public async Task<Trip?> GetNextAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (tripRepository.GetCurrent(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        
        await foreach (var trip in trips)
        {
            var work = await workRepository.GetAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            
            logger.LogDebug("Trip ({@Trip}) is now has the earliest StartTime ({StartTime}) for its Work ({@Work}) with WorkType ({WorkType}).",
                trip,
                earliestStart,
                work,
                workType);
        }
        
        logger.LogInformation("Trip ({@Trip}) has the earliest StartTime ({StartTime}) for its Work with WorkType ({WorkType}).",
            nextTrip,
            earliestStart,
            workType);
        return nextTrip;
    }
    
    public async Task AlertFreeAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work != null)
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned " +
                            "and can therefore not claim this Truck ({@Truck}).",
                trip,
                work,
                truck);
            
            return;
        }
        
        logger.LogDebug("Setting this Truck ({@Truck}) to this Trip ({@Trip})...",
            truck,
            trip);
        await tripRepository.SetAsync(trip, truck, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip ({@Trip})...",
            WorkType.TravelHub,
            trip);
        await workService.AddAsync(trip, WorkType.TravelHub, cancellationToken);
        
        logger.LogInformation("Truck ({@Truck}) successfully linked to this Trip ({@Trip}).",
            truck,
            trip);
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitParking })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this ParkingSpot ({@ParkingSpot}).",
                trip,
                work,
                WorkType.WaitParking,
                parkingSpot);
            
            return;
        }
        
        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Setting this ParkingSpot ({@ParkingSpot}) to this Trip ({@Trip})...",
            parkingSpot,
            trip);
        await tripRepository.SetAsync(trip, parkingSpot, cancellationToken);
        
        logger.LogDebug("Setting ParkingSpot ({@ParkingSpot}) location to this Trip ({@Trip})...",
            parkingSpot,
            trip);
        await locationService.SetAsync(trip, parkingSpot, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip ({@Trip})...",
            WorkType.WaitCheckIn,
            trip);
        await workService.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);
        
        logger.LogInformation("ParkingSpot ({@ParkingSpot}) successfully linked to this Trip ({@Trip}).",
            parkingSpot,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitCheckIn })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this AdminStaff ({@AdminStaff}).",
                trip,
                work,
                WorkType.WaitCheckIn,
                adminStaff);
            
            return;
        }

        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Setting this AdminStaff ({@AdminStaff}) to this Trip ({@Trip})...",
            adminStaff,
            trip);
        await tripRepository.SetAsync(trip, adminStaff, cancellationToken);
        
        logger.LogDebug("Adding Work for this AdminStaff ({@AdminStaff}) to this Trip ({@Trip})...",
            adminStaff,
            trip);
        await workService.AddAsync(trip, adminStaff, cancellationToken);
        
        logger.LogInformation("AdminStaff ({@AdminStaff}) successfully linked to this Trip ({@Trip}).",
            adminStaff,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitBay })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not claim this Bay ({@Bay}).",
                trip,
                work,
                WorkType.WaitBay,
                bay);
            
            return;
        }
        
        
        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var parkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (parkingSpot == null)
        {
            logger.LogError("Trip ({@Trip}) has no ParkingSpot assigned to complete its Wait for a Bay at.",
                trip);
        }
        else
        {
            logger.LogDebug("Removing ParkingSpot ({@ParkingSpot}) from this Trip ({@Trip})...",
                parkingSpot,
                trip);
            await tripRepository.UnsetAsync(trip, parkingSpot, cancellationToken);
            logger.LogInformation("ParkingSpot ({@ParkingSpot}) successfully removed from this Trip ({@Trip}).",
                parkingSpot,
                trip);
        }
        
        logger.LogDebug("Setting this Bay ({@Bay}) to this Trip ({@Trip})...",
            bay,
            trip);
        await tripRepository.SetAsync(trip, bay, cancellationToken);
        
        logger.LogDebug("Setting Bay ({@Bay}) location to this Trip ({@Trip})...",
            bay,
            trip);
        await locationService.SetAsync(trip, bay, cancellationToken);
        
        logger.LogDebug("Setting the BayStatus of this Bay ({@Bay}) to {BayStatus}...",
            bay,
            BayStatus.Claimed);
        await bayRepository.SetAsync(bay, BayStatus.Claimed, cancellationToken);
        
        logger.LogDebug("Adding Work for this Bay ({@Bay}) to this Trip ({@Trip})...",
            bay,
            trip);
        await workService.AddAsync(trip, bay, cancellationToken);
        
        logger.LogInformation("Bay ({@Bay}) successfully linked to this Trip ({@Trip}).",
            bay,
            trip);
    }
    
    public async Task AlertTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHub })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its travel to the Hub completed.",
                trip,
                work,
                WorkType.TravelHub);
            
            return;
        }

        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip ({@Trip})...",
            WorkType.WaitParking,
            trip);
        await workService.AddAsync(trip, WorkType.WaitParking, cancellationToken);
        
        logger.LogInformation("Trip ({@Trip}) successfully arrived at the Hub.",
            trip);
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.CheckIn })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Check-In completed.",
                trip,
                work,
                WorkType.CheckIn);
            
            return;
        }
        
        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var adminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
        if (adminStaff == null)
        {
            logger.LogError("Trip ({@Trip}) has no AdminStaff assigned to complete its Check-In at...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing AdminStaff ({@AdminStaff}) from this Trip ({@Trip})...",
                adminStaff,
                trip);
            await tripRepository.UnsetAsync(trip, adminStaff, cancellationToken);
            logger.LogInformation("AdminStaff ({@AdminStaff}) successfully removed from this Trip ({@Trip}).",
                adminStaff,
                trip);
        }
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip ({@Trip})...",
            WorkType.WaitBay,
            trip);
        await workService.AddAsync(trip, WorkType.WaitBay, cancellationToken);
        
        logger.LogInformation("Trip ({@Trip}) successfully completed Check-In.",
            trip);
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.Bay })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Bay Work completed.",
                trip,
                work,
                WorkType.Bay);
            
            return;
        }

        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        var bay = await bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            logger.LogError("Trip ({@Trip}) has no Bay assigned to complete its Bay Work at...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing Bay ({@Bay}) from this Trip ({@Trip})...",
                bay,
                trip);
            await tripRepository.UnsetAsync(trip, bay, cancellationToken);
            logger.LogInformation("Bay ({@Bay}) successfully removed from this Trip ({@Trip}).",
                bay,
                trip);
        }
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip ({@Trip})...",
            WorkType.TravelHome,
            trip);
        await workService.AddAsync(trip, WorkType.TravelHome, cancellationToken);
        
        logger.LogInformation("Trip ({@Trip}) successfully completed Bay Work.",
            trip);
    }
    
    public async Task AlertTravelHomeCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHome })
        {
            logger.LogError("Trip ({@Trip}) has active Work ({@Work}) assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Travel Home completed.",
                trip,
                work,
                WorkType.TravelHome);
            
            return;
        }
        
        logger.LogDebug("Removing active Work ({@Work}) assigned to this Trip ({@Trip})...",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            logger.LogError("Trip ({@Trip}) has no Truck assigned to complete its Travel Home with...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing Truck ({@Truck}) from this Trip ({@Trip})...",
                truck,
                trip);
            await tripRepository.UnsetAsync(trip, truck, cancellationToken);
            logger.LogInformation("Truck ({@Truck}) successfully removed from this Trip ({@Trip}).",
                truck,
                trip);
        }
        
        logger.LogInformation("Trip ({@Trip}) successfully COMPLETED!!!",
            trip);
    }
    
    public async Task TravelAsync(Trip trip, Truck truck, long xDestination, long yDestination, CancellationToken cancellationToken)
    {
        var xDiff = xDestination - trip.XLocation;
        var xTravel = xDiff > truck.Speed ? truck.Speed : xDiff;
        var newXLocation = trip.XLocation + xTravel;
        
        var yDiff = yDestination - trip.YLocation;
        var yTravel = yDiff > truck.Speed ? truck.Speed : yDiff;
        var newYLocation = trip.YLocation + yTravel;

        logger.LogDebug("Setting new location (XLocation={XLocation} YLocation={YLocation}) to this Trip ({@Trip})...",
            newXLocation,
            newYLocation,
            trip);
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
        {
            logger.LogError("Trip ({@Trip}) has no Truck assigned to travel to the Hub with.",
                trip);
            
            return;
        }
        
        var hub = await hubRepository.GetAsync(trip, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Trip ({@Trip}) has no Hub assigned to travel to.",
                trip);
            
            return;
        }
        
        await TravelAsync(trip, truck, hub, cancellationToken);
    }

    public async Task TravelHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            logger.LogError("Trip ({@Trip}) has no Truck assigned to travel home with.",
                trip);
            
            return;
        }
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("Trip ({@Trip}) has no TruckCompany assigned to travel home to, poor thing...",
                trip);
            
            return;
        }
        
        await TravelAsync(trip, truck, truckCompany, cancellationToken);
    }


    public async Task<bool> IsAtHubAsync(Trip trip, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(trip, cancellationToken);
        if (hub != null)
            return hub.XLocation == trip.XLocation &&
                   hub.YLocation == trip.YLocation;
        
        logger.LogError("Trip ({@Trip}) has no Hub assigned to travel to.",
            trip);
        
        return false;

    }

    public async Task<bool> IsAtHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            logger.LogError("Trip ({@Trip}) has no Truck assigned to travel home with.",
                trip);
            
            return false;
        }
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany != null)
            return truckCompany.XLocation == trip.XLocation &&
                   truckCompany.YLocation == trip.YLocation;
        
        logger.LogError("Trip ({@Trip}) has no TruckCompany assigned to travel home to, poor thing...",
            trip);
            
        return false;

    }
}
