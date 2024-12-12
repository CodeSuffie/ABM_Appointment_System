using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.AppointmentServices;
using Services.LoadServices;
using Services.ModelServices;
using Services.PelletServices;
using Services.TruckServices;

namespace Services.TripServices;

public sealed class TripService
{
    private readonly ILogger<TripService> _logger;
    private readonly LoadService _loadService;
    private readonly WorkRepository _workRepository;
    private readonly ParkingSpotRepository _parkingSpotRepository;
    private readonly AdminStaffRepository _adminStaffRepository;
    private readonly TripRepository _tripRepository;
    private readonly HubRepository _hubRepository;
    private readonly BayRepository _bayRepository;
    private readonly TruckRepository _truckRepository;
    private readonly LocationService _locationService;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly WorkService _workService;
    private readonly PelletService _pelletService;
    private readonly TruckService _truckService;
    private readonly AppointmentService _appointmentService;
    private readonly ModelState _modelState;

    public TripService(ILogger<TripService> logger,
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
        PelletService pelletService,
        TruckService truckService,
        AppointmentService appointmentService,
        ModelState modelState)
    {
        _logger = logger;
        _loadService = loadService;
        _workRepository = workRepository;
        _parkingSpotRepository = parkingSpotRepository;
        _adminStaffRepository = adminStaffRepository;
        _tripRepository = tripRepository;
        _hubRepository = hubRepository;
        _bayRepository = bayRepository;
        _truckRepository = truckRepository;
        _locationService = locationService;
        _truckCompanyRepository = truckCompanyRepository;
        _workService = workService;
        _pelletService = pelletService;
        _truckService = truckService;
        _appointmentService = appointmentService;
        _modelState = modelState;
    }

    public async Task<Trip?> GetNextAsync(Truck truck, CancellationToken cancellationToken)
    {
        while (true)
        {
            var dropOff = await _loadService.GetNewDropOffAsync(truck, cancellationToken);
            Load? pickUp;
            Trip? trip = null;
            Hub? hub = null;
    
            if (dropOff != null)
            {
                trip = new Trip
                {
                    Completed = false
                };
                
                await _tripRepository.AddAsync(trip, cancellationToken);
                await _tripRepository.SetDropOffAsync(trip, dropOff, cancellationToken);
                await _pelletService.LoadPelletsAsync(truck, dropOff, cancellationToken);
    
                hub = await _hubRepository.GetAsync(dropOff, cancellationToken);
                if (hub == null)
                {
                    _logger.LogError("Drop-Off Load \n({@Load})\n for this Truck \n({@Truck})\n did not have a Hub assigned.",
                        dropOff,
                        truck);
    
                    continue;
                }
                
                pickUp = await _loadService.GetNewPickUpAsync(truck, hub, cancellationToken);

                if (pickUp != null)
                {
                    await _tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
            }
            else
            {
                pickUp = await _loadService.GetNewPickUpAsync(truck, cancellationToken);
                if (pickUp != null)
                {
                    trip = new Trip
                    {
                        Completed = false
                    };
                    
                    hub = await _hubRepository.GetAsync(pickUp, cancellationToken);
                    if (hub == null)
                    {
                        _logger.LogError("Pick-Up Load \n({@Load})\n for this Truck \n({@Truck})\n did not have a Hub assigned.",
                            pickUp,
                            truck);
    
                        continue;
                    }
                    
                    await _tripRepository.AddAsync(trip, cancellationToken);
                    await _tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
            }

            if (trip == null) return trip;
            
            var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) to create the new Trip for.",
                    truck);

                return null;
            }
                
            _logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n location to this Trip \n({@Trip})",
                truckCompany,
                trip);
            await _locationService.SetAsync(trip, truckCompany, cancellationToken);
        
            if (hub == null)
            {
                _logger.LogError("No Hub was assigned to the Trip to create.");

                return null;
            }

            var earliestArrivalTime = _modelState.ModelTime + _truckService.GetTravelTime(truck, truckCompany, hub);
            await _appointmentService.SetAsync(trip, hub, earliestArrivalTime);

            return trip;
        }
    }

    public async Task<Trip?> GetNextAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (_tripRepository.GetCurrent(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        
        await foreach (var trip in trips)
        {
            var work = await _workRepository.GetAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            
            _logger.LogDebug("Trip \n({@Trip})\n is now has the earliest StartTime \n({StartTime})\n for its Work \n({@Work})\n with WorkType \n({WorkType})",
                trip,
                earliestStart,
                work,
                workType);

            nextTrip = trip;
            earliestStart = work?.StartTime;
        }
        
        _logger.LogInformation("Trip \n({@Trip})\n has the earliest StartTime \n({StartTime})\n for its Work with WorkType \n({WorkType})",
            nextTrip,
            earliestStart,
            workType);
        return nextTrip;
    }
    
    public async Task AlertFreeAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work != null)
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned " +
                            "and can therefore not claim this Truck \n({@Truck})",
                trip,
                work,
                truck);
            
            return;
        }
        
        _logger.LogDebug("Setting this Truck \n({@Truck})\n to this Trip \n({@Trip})",
            truck,
            trip);
        await _tripRepository.SetAsync(trip, truck, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitTravelHub,
            trip);
        await _workService.AddAsync(trip, WorkType.WaitTravelHub, cancellationToken);
        
        _logger.LogInformation("Truck \n({@Truck})\n successfully linked to this Trip \n({@Trip})",
            truck,
            trip);
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitParking })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this ParkingSpot \n({@ParkingSpot})",
                trip,
                work,
                WorkType.WaitParking,
                parkingSpot);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Setting this ParkingSpot \n({@ParkingSpot})\n to this Trip \n({@Trip})",
            parkingSpot,
            trip);
        await _tripRepository.SetAsync(trip, parkingSpot, cancellationToken);
        
        _logger.LogDebug("Setting ParkingSpot \n({@ParkingSpot})\n location to this Trip \n({@Trip})",
            parkingSpot,
            trip);
        await _locationService.SetAsync(trip, parkingSpot, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitCheckIn,
            trip);
        await _workService.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);
        
        _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully linked to this Trip \n({@Trip})",
            parkingSpot,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitCheckIn })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}" +
                            "and can therefore not claim this AdminStaff \n({@AdminStaff})",
                trip,
                work,
                WorkType.WaitCheckIn,
                adminStaff);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Setting this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})",
            adminStaff,
            trip);
        await _tripRepository.SetAsync(trip, adminStaff, cancellationToken);
        
        _logger.LogDebug("Adding Work for this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})",
            adminStaff,
            trip);
        await _workService.AddAsync(trip, adminStaff, cancellationToken);
        
        _logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully linked to this Trip \n({@Trip})",
            adminStaff,
            trip);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitBay })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not claim this Bay \n({@Bay})",
                trip,
                work,
                WorkType.WaitBay,
                bay);
            
            return;
        }
        
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);

        var parkingSpot = await _parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (parkingSpot == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no ParkingSpot assigned to complete its Wait for a Bay at.",
                trip);
        }
        else
        {
            _logger.LogDebug("Removing ParkingSpot \n({@ParkingSpot})\n from this Trip \n({@Trip})",
                parkingSpot,
                trip);
            await _tripRepository.UnsetAsync(trip, parkingSpot, cancellationToken);
            _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully removed from this Trip \n({@Trip})",
                parkingSpot,
                trip);
        }
        
        _logger.LogDebug("Setting this Bay \n({@Bay})\n to this Trip \n({@Trip})",
            bay,
            trip);
        await _tripRepository.SetAsync(trip, bay, cancellationToken);
        
        _logger.LogDebug("Setting Bay \n({@Bay})\n location to this Trip \n({@Trip})",
            bay,
            trip);
        await _locationService.SetAsync(trip, bay, cancellationToken);
        
        _logger.LogDebug("Adding Work for this Bay \n({@Bay})\n to this Trip \n({@Trip})",
            bay,
            trip);
        await _workService.AddAsync(trip, bay, cancellationToken);
        
        _logger.LogInformation("Bay \n({@Bay})\n successfully linked to this Trip \n({@Trip})",
            bay,
            trip);
    }
    
    public async Task AlertTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHub })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its travel to the Hub completed.",
                trip,
                work,
                WorkType.TravelHub);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitParking,
            trip);
        await _workService.AddAsync(trip, WorkType.WaitParking, cancellationToken);
        
        _logger.LogInformation("Trip \n({@Trip})\n successfully arrived at the Hub.",
            trip);
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.CheckIn })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Check-In completed.",
                trip,
                work,
                WorkType.CheckIn);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);

        var adminStaff = await _adminStaffRepository.GetAsync(trip, cancellationToken);
        if (adminStaff == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no AdminStaff assigned to complete its Check-In at...",
                trip);
        }
        else
        {
            _logger.LogDebug("Removing AdminStaff \n({@AdminStaff})\n from this Trip \n({@Trip})",
                adminStaff,
                trip);
            await _tripRepository.UnsetAsync(trip, adminStaff, cancellationToken);
            _logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully removed from this Trip \n({@Trip})",
                adminStaff,
                trip);
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.WaitBay,
            trip);
        await _workService.AddAsync(trip, WorkType.WaitBay, cancellationToken);
        
        _logger.LogInformation("Trip \n({@Trip})\n successfully completed Check-In.",
            trip);
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.Bay })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Bay Work completed.",
                trip,
                work,
                WorkType.Bay);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no Bay assigned to complete its Bay Work at...",
                trip);
        }
        else
        {
            _logger.LogDebug("Removing Bay \n({@Bay})\n from this Trip \n({@Trip})",
                bay,
                trip);
            await _tripRepository.UnsetAsync(trip, bay, cancellationToken);
            _logger.LogInformation("Bay \n({@Bay})\n successfully removed from this Trip \n({@Trip})",
                bay,
                trip);
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})",
            WorkType.TravelHome,
            trip);
        await _workService.AddAsync(trip, WorkType.TravelHome, cancellationToken);
        
        _logger.LogInformation("Trip ({@Trip})\n successfully completed Bay Work.",
            trip);
    }
    
    public async Task AlertTravelHomeCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHome })
        {
            _logger.LogError("Trip ({@Trip})\n has active Work ({@Work})\n assigned with WorkType not of type {@WorkType} " +
                            "and can therefore not be alerted to have its Travel Home completed.",
                trip,
                work,
                WorkType.TravelHome);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work ({@Work})\n assigned to this Trip ({@Trip})",
            work,
            trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        await CompleteTripAsync(trip, cancellationToken);
    }

    private async Task CompleteTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        await _tripRepository.SetAsync(trip, true, cancellationToken);
        await _pelletService.CompleteAsync(trip, cancellationToken);

        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to complete with...",
                trip);
        }
        else
        {
            _logger.LogDebug("Removing Truck ({@Truck})\n from this Trip ({@Trip})",
                truck,
                trip);
            await _tripRepository.UnsetAsync(trip, truck, cancellationToken);
            _logger.LogInformation("Truck ({@Truck})\n successfully removed from this Trip ({@Trip})",
                truck,
                trip);
        }
        
        _logger.LogInformation("Trip ({@Trip})\n successfully COMPLETED!!!",
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

        _logger.LogDebug("Setting new location (XLocation={XLocation} YLocation={YLocation})\n to this Trip ({@Trip})",
            newXLocation,
            newYLocation,
            trip);
        await _locationService.SetAsync(trip, newXLocation, newYLocation, cancellationToken);
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
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel to the Hub with.",
                trip);
            
            return;
        }
        
        var hub = await _hubRepository.GetAsync(trip, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.",
                trip);
            
            return;
        }
        
        await TravelAsync(trip, truck, hub, cancellationToken);
    }

    public async Task TravelHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.",
                trip);
            
            return;
        }
        
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...",
                trip);
            
            return;
        }
        
        await TravelAsync(trip, truck, truckCompany, cancellationToken);
    }


    public async Task<bool> IsAtHubAsync(Trip trip, CancellationToken cancellationToken)
    {
        var hub = await _hubRepository.GetAsync(trip, cancellationToken);
        if (hub != null)
            return hub.XLocation == trip.XLocation &&
                   hub.YLocation == trip.YLocation;
        
        _logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.",
            trip);
        
        return false;

    }

    public async Task<bool> IsAtHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.",
                trip);
            
            return false;
        }
        
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany != null)
            return truckCompany.XLocation == trip.XLocation &&
                   truckCompany.YLocation == trip.YLocation;
        
        _logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...",
            trip);
            
        return false;

    }
}
