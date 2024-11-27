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
            Load? pickUp = null;
            Trip? trip = null;
    
            if (dropOff != null)
            {
                trip = new Trip
                {
                    Completed = false
                };
                
                await tripRepository.AddAsync(trip, cancellationToken);
                await tripRepository.SetDropOffAsync(trip, dropOff, cancellationToken);
    
                var dropOffHub = await hubRepository.GetAsync(dropOff, cancellationToken);
                if (dropOffHub == null)
                {
                    logger.LogError("Drop-Off Load \n({@Load})\n for this TruckCompany \n({@TruckCompany})\n did not have a Hub assigned.",
                        dropOff,
                        truckCompany);
    
                    logger.LogDebug("Removing invalid Drop-Off Load \n({@Load})\n for this TruckCompany \n({@TruckCompany})",
                        dropOff,
                        truckCompany);
                    await loadRepository.RemoveAsync(dropOff, cancellationToken);
    
                    continue;
                }
                
                pickUp = await loadService.SelectUnclaimedPickUpAsync(dropOffHub, truckCompany, cancellationToken);

                if (pickUp != null)
                {
                    await tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
            }
            else
            {
                pickUp = await loadService.SelectUnclaimedPickUpAsync(truckCompany, cancellationToken);
                if (pickUp != null)
                {
                    trip = new Trip
                    {
                        Completed = false
                    };
                    await tripRepository.AddAsync(trip, cancellationToken);
                    await tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
            }

            if (trip != null)
            {
                logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n location to this Trip \n({@Trip})",
                                truckCompany,
                                trip);
                            await locationService.SetAsync(trip, truckCompany, cancellationToken);
            }

            return trip;
        }
    }

    public async Task AddNewObjectsAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trip = await GetNewObjectAsync(truckCompany, cancellationToken);
        while (trip != null)
        {
            logger.LogInformation("New Trip created for this TruckCompany \n({@TruckCOmpany})\n: Trip={@Trip}",
                truckCompany,
                trip);
            
            trip = await GetNewObjectAsync(truckCompany, cancellationToken);
        }
        
        logger.LogInformation("TruckCompany \n({@TruckCompany})\n could not construct any more new Trips...",
            truckCompany);
    }
    
    public async Task<Trip?> GetNextAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trip = await GetNewObjectAsync(truckCompany, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("TruckCompany \n({@TruckCompany})\n could not create a Trip.",
                truckCompany);

            return null;
        }

        if (trip.Truck != null)
        {
            logger.LogError("Trip \n({@Trip})\n already has a Truck assigned ({@Truck}).",
                trip,
                trip.Truck);

            return null;
        }

        if (trip.Completed)
        {
            logger.LogError("Trip \n({@Trip})\n was already completed.",
                trip);

            return null;
        }
        
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
            
            logger.LogDebug("Trip \n({@Trip})\n is now has the earliest StartTime \n({StartTime})\n for its Work \n({@Work})\n with WorkType \n({WorkType})",
                trip,
                earliestStart,
                work,
                workType);

            nextTrip = trip;
            earliestStart = work?.StartTime;
        }
        
        logger.LogInformation("Trip \n({@Trip})\n has the earliest StartTime \n({StartTime})\n for its Work with WorkType \n({WorkType})",
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
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned " +
                            "and can therefore not claim this Truck \n({@Truck})",
                trip,
                work,
                truck);
            
            return;
        }
        
        logger.LogDebug("Setting this Truck \n({@Truck})\n to this Trip \n({@Trip})",
            truck,
            trip);
        await tripRepository.SetAsync(trip, truck, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.TravelHub,
            trip);
        await workService.AddAsync(trip, WorkType.TravelHub, cancellationToken);
        
        logger.LogInformation("Truck \n({@Truck})\n successfully linked to this Trip \n({@Trip})",
            truck,
            trip);
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitParking })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this ParkingSpot \n({@ParkingSpot})",
                trip,
                work,
                WorkType.WaitParking,
                parkingSpot);
            
            return;
        }
        
        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Setting this ParkingSpot \n({@ParkingSpot})\n to this Trip \n({@Trip})",
            parkingSpot,
            trip);
        await tripRepository.SetAsync(trip, parkingSpot, cancellationToken);
        
        logger.LogDebug("Setting ParkingSpot \n({@ParkingSpot})\n location to this Trip \n({@Trip})",
            parkingSpot,
            trip);
        await locationService.SetAsync(trip, parkingSpot, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitCheckIn,
            trip);
        await workService.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);
        
        logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully linked to this Trip \n({@Trip})",
            parkingSpot,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitCheckIn })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this AdminStaff \n({@AdminStaff})",
                trip,
                work,
                WorkType.WaitCheckIn,
                adminStaff);
            
            return;
        }

        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Setting this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})",
            adminStaff,
            trip);
        await tripRepository.SetAsync(trip, adminStaff, cancellationToken);
        
        logger.LogDebug("Adding Work for this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})",
            adminStaff,
            trip);
        await workService.AddAsync(trip, adminStaff, cancellationToken);
        
        logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully linked to this Trip \n({@Trip})",
            adminStaff,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitBay })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not claim this Bay \n({@Bay})",
                trip,
                work,
                WorkType.WaitBay,
                bay);
            
            return;
        }
        
        
        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var parkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (parkingSpot == null)
        {
            logger.LogError("Trip \n({@Trip})\n has no ParkingSpot assigned to complete its Wait for a Bay at.",
                trip);
        }
        else
        {
            logger.LogDebug("Removing ParkingSpot \n({@ParkingSpot})\n from this Trip \n({@Trip})",
                parkingSpot,
                trip);
            await tripRepository.UnsetAsync(trip, parkingSpot, cancellationToken);
            logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully removed from this Trip \n({@Trip})",
                parkingSpot,
                trip);
        }
        
        logger.LogDebug("Setting this Bay \n({@Bay})\n to this Trip \n({@Trip})",
            bay,
            trip);
        await tripRepository.SetAsync(trip, bay, cancellationToken);
        
        logger.LogDebug("Setting Bay \n({@Bay})\n location to this Trip \n({@Trip})",
            bay,
            trip);
        await locationService.SetAsync(trip, bay, cancellationToken);
        
        logger.LogDebug("Setting the BayStatus of this Bay \n({@Bay})\n to {BayStatus}...",
            bay,
            BayStatus.Claimed);
        await bayRepository.SetAsync(bay, BayStatus.Claimed, cancellationToken);
        
        logger.LogDebug("Adding Work for this Bay \n({@Bay})\n to this Trip \n({@Trip})",
            bay,
            trip);
        await workService.AddAsync(trip, bay, cancellationToken);
        
        logger.LogInformation("Bay \n({@Bay})\n successfully linked to this Trip \n({@Trip})",
            bay,
            trip);
    }
    
    public async Task AlertTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHub })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its travel to the Hub completed.",
                trip,
                work,
                WorkType.TravelHub);
            
            return;
        }

        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitParking,
            trip);
        await workService.AddAsync(trip, WorkType.WaitParking, cancellationToken);
        
        logger.LogInformation("Trip \n({@Trip})\n successfully arrived at the Hub.",
            trip);
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.CheckIn })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Check-In completed.",
                trip,
                work,
                WorkType.CheckIn);
            
            return;
        }
        
        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var adminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
        if (adminStaff == null)
        {
            logger.LogError("Trip \n({@Trip})\n has no AdminStaff assigned to complete its Check-In at...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing AdminStaff \n({@AdminStaff})\n from this Trip \n({@Trip})",
                adminStaff,
                trip);
            await tripRepository.UnsetAsync(trip, adminStaff, cancellationToken);
            logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully removed from this Trip \n({@Trip})",
                adminStaff,
                trip);
        }
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitBay,
            trip);
        await workService.AddAsync(trip, WorkType.WaitBay, cancellationToken);
        
        logger.LogInformation("Trip \n({@Trip})\n successfully completed Check-In.",
            trip);
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.Bay })
        {
            logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Bay Work completed.",
                trip,
                work,
                WorkType.Bay);
            
            return;
        }

        logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);
        
        var bay = await bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            logger.LogError("Trip \n({@Trip})\n has no Bay assigned to complete its Bay Work at...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing Bay \n({@Bay})\n from this Trip \n({@Trip})",
                bay,
                trip);
            await tripRepository.UnsetAsync(trip, bay, cancellationToken);
            logger.LogInformation("Bay \n({@Bay})\n successfully removed from this Trip \n({@Trip})",
                bay,
                trip);
        }
        
        logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.TravelHome,
            trip);
        await workService.AddAsync(trip, WorkType.TravelHome, cancellationToken);
        
        logger.LogInformation("Trip ({@Trip})\n successfully completed Bay Work.",
            trip);
    }
    
    public async Task AlertTravelHomeCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHome })
        {
            logger.LogError("Trip ({@Trip})\n has active Work ({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Travel Home completed.",
                trip,
                work,
                WorkType.TravelHome);
            
            return;
        }
        
        logger.LogDebug("Removing active Work ({@Work})\n assigned to this Trip ({@Trip})",
            work,
            trip);
        await workRepository.RemoveAsync(work, cancellationToken);

        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            logger.LogError("Trip ({@Trip})\n has no Truck assigned to complete its Travel Home with...",
                trip);
        }
        else
        {
            logger.LogDebug("Removing Truck ({@Truck})\n from this Trip ({@Trip})",
                truck,
                trip);
            await tripRepository.UnsetAsync(trip, truck, cancellationToken);
            logger.LogInformation("Truck ({@Truck})\n successfully removed from this Trip ({@Trip})",
                truck,
                trip);
        }

        await tripRepository.SetAsync(trip, true, cancellationToken);
        logger.LogInformation("Trip ({@Trip})\n successfully COMPLETED!!!",
            trip);
    }
    
    public async Task TravelAsync(Trip trip, Truck truck, long xDestination, long yDestination, CancellationToken cancellationToken)
    {
        var xDiff = xDestination - trip.XLocation;
        var xTravel = Math.Clamp(xDiff, -truck.Speed, truck.Speed);
        var newXLocation = trip.XLocation + xTravel;
        
        var yDiff = yDestination - trip.YLocation;
        var yTravel = Math.Clamp(yDiff, -truck.Speed, truck.Speed);
        var newYLocation = trip.YLocation + yTravel;

        logger.LogDebug("Setting new location (XLocation={XLocation} YLocation={YLocation})\n to this Trip ({@Trip})",
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
            logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel to the Hub with.",
                trip);
            
            return;
        }
        
        var hub = await hubRepository.GetAsync(trip, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.",
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
            logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.",
                trip);
            
            return;
        }
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...",
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
        
        logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.",
            trip);
        
        return false;

    }

    public async Task<bool> IsAtHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.",
                trip);
            
            return false;
        }
        
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany != null)
            return truckCompany.XLocation == trip.XLocation &&
                   truckCompany.YLocation == trip.YLocation;
        
        logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...",
            trip);
            
        return false;

    }
}
