using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;
using Settings;

namespace Services;

public sealed class TripService
{
    private readonly ILogger<TripService> _logger;
    private readonly LoadFactory _loadFactory;
    private readonly WorkRepository _workRepository;
    private readonly ParkingSpotRepository _parkingSpotRepository;
    private readonly AdminStaffRepository _adminStaffRepository;
    private readonly TripRepository _tripRepository;
    private readonly TripFactory _tripFactory;
    private readonly HubRepository _hubRepository;
    private readonly BayRepository _bayRepository;
    private readonly TruckRepository _truckRepository;
    private readonly LocationFactory _locationFactory;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly WorkFactory _workFactory;
    private readonly PalletService _palletService;
    private readonly AppointmentSlotRepository _appointmentSlotRepository;
    private readonly AppointmentRepository _appointmentRepository;
    private readonly ModelState _modelState;
    private readonly Instrumentation _instrumentation;

    public TripService(ILogger<TripService> logger,
        LoadFactory loadFactory,
        WorkRepository workRepository,
        ParkingSpotRepository parkingSpotRepository,
        AdminStaffRepository adminStaffRepository,
        TripRepository tripRepository,
        TripFactory tripFactory,
        HubRepository hubRepository,
        BayRepository bayRepository,
        TruckRepository truckRepository,
        LocationFactory locationFactory,
        TruckCompanyRepository truckCompanyRepository,
        WorkFactory workFactory,
        PalletService palletService,
        AppointmentSlotRepository appointmentSlotRepository,
        AppointmentRepository appointmentRepository,
        ModelState modelState,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _loadFactory = loadFactory;
        _workRepository = workRepository;
        _parkingSpotRepository = parkingSpotRepository;
        _adminStaffRepository = adminStaffRepository;
        _tripRepository = tripRepository;
        _tripFactory = tripFactory;
        _hubRepository = hubRepository;
        _bayRepository = bayRepository;
        _truckRepository = truckRepository;
        _locationFactory = locationFactory;
        _truckCompanyRepository = truckCompanyRepository;
        _workFactory = workFactory;
        _palletService = palletService;
        _appointmentSlotRepository = appointmentSlotRepository;
        _appointmentRepository = appointmentRepository;
        _modelState = modelState;
        _instrumentation = instrumentation;
    }

    public async Task<Trip?> GetNextAsync(Truck truck, CancellationToken cancellationToken)
    {
        while (true)
        {
            _logger.LogDebug("Finding DropOff Load for this Truck \n({@Truck}).", truck);
            var dropOff = await _loadFactory.GetNewDropOffAsync(truck, cancellationToken);
            
            Load? pickUp = null;
            Trip? trip = null;
            Hub? hub = null;
    
            if (dropOff != null)
            {
                _logger.LogInformation("Found DropOff Load \n({@Load})\n for this Truck \n({@Truck}).", dropOff, truck);
    
                hub = await _hubRepository.GetAsync(dropOff, cancellationToken);
                if (hub == null)
                {
                    _logger.LogError("Drop-Off Load \n({@Load})\n for this Truck \n({@Truck})\n did not have a Hub assigned.", dropOff, truck);
    
                    continue;
                }

                trip = await _tripFactory.GetNewObjectAsync(truck, hub, cancellationToken);
                if (trip == null)
                {
                    _logger.LogInformation("Trip could not be created for this Truck \n({@Truck})\n with this DropOff Load \n({@Load})\n and this Hub \n({@Hub}).", truck, dropOff, hub);

                    continue;
                }
                
                _logger.LogDebug("Setting DropOff Load \n({@Load})\n to this Trip \n({@Trip}).", dropOff, trip);
                await _tripRepository.SetDropOffAsync(trip, dropOff, cancellationToken);
        
                _logger.LogDebug("Loading Pallets for DropOff Load \n({@Load})\n onto this Truck \n({@Truck}).", dropOff, truck);
                await _palletService.LoadPalletsAsync(truck, dropOff, cancellationToken);
                
                _logger.LogDebug("Finding PickUp Load for this Truck \n({@Truck})\n with this Trip \n({@Trip}).", truck, trip);
                pickUp = await _loadFactory.GetNewPickUpAsync(truck, hub, cancellationToken);

                if (pickUp != null)
                {
                    _logger.LogInformation("Found PickUp Load \n({@Load})\n for this Truck \n({@Truck})\n with this Trip \n({@Trip}).", pickUp, truck, trip);
                    
                    _logger.LogDebug("Setting PickUp Load \n({@Load})\n to this Trip \n({@Trip}).", dropOff, trip);
                    await _tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("No PickUp Load could be found for this Truck \n({@Truck})\n with this Trip \n({@Trip}).", truck, trip);
                }
            }
            else
            {
                _logger.LogInformation("No DropOff Load could be found for this Truck \n({@Truck}).", truck);
                
                _logger.LogDebug("Finding PickUp Load for this Truck \n({@Truck}).", truck);
                pickUp = await _loadFactory.GetNewPickUpAsync(truck, cancellationToken);
                
                if (pickUp != null)
                {
                    _logger.LogInformation("Found PickUp Load \n({@Load})\n for this Truck \n({@Truck}).", pickUp, truck);
                    
                    hub = await _hubRepository.GetAsync(pickUp, cancellationToken);
                    if (hub == null)
                    {
                        _logger.LogError("Pick-Up Load \n({@Load})\n for this Truck \n({@Truck})\n did not have a Hub assigned.", pickUp, truck);
    
                        continue;
                    }
                    
                    trip = await _tripFactory.GetNewObjectAsync(truck, hub, cancellationToken);
                    if (trip == null)
                    {
                        _logger.LogInformation("Trip could not be created for this Truck \n({@Truck})\n with this PickUp Load \n({@Load})\n and this Hub \n({@Hub}).", truck, pickUp, hub);

                        continue;
                    }
                    
                    _logger.LogDebug("Setting PickUp Load \n({@Load})\n to this Trip \n({@Trip}).", dropOff, trip);
                    await _tripRepository.SetPickUpAsync(trip, pickUp, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Trip could not be created for this Truck \n({@Truck})\n and this Hub \n({@Hub}).", truck, hub);

                    return null;
                }
            }

            return trip;
        }
    }

    private async Task<Trip?> GetNextBaseAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (_tripRepository.GetCurrent(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        
        await foreach (var trip in trips)
        {
            var work = await _workRepository.GetAsync(trip, cancellationToken);
            if (work == null) continue;
            
            if (nextTrip != null && work.StartTime > earliestStart) continue;
            
            _logger.LogDebug("Trip \n({@Trip})\n is now has the earliest StartTime ({Step})\n for its Work \n({@Work})\n with WorkType \n({WorkType})", trip, earliestStart, work, workType);

            nextTrip = trip;
            earliestStart = work.StartTime;
        }
        
        _logger.LogInformation("Trip \n({@Trip})\n has the earliest StartTime ({Step})\n for its Work with WorkType \n({WorkType})", nextTrip, earliestStart, workType);
        return nextTrip;
    }
    
    private async Task<Trip?> GetNextAppointmentAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        if (workType == WorkType.WaitBay)
        {
            _logger.LogError("Getting next Trip failed due to WorkType ({WorkType}) being of an invalid Type ({WorkTypeInvalid}).", workType, WorkType.WaitBay);

            return null;
        }
        
        var trips = (_tripRepository.GetCurrent(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestAppointmentTime = null;
        
        await foreach (var trip in trips)
        {
            var appointment = await _appointmentRepository.GetAsync(trip, cancellationToken);
            if (appointment == null)
            {
                _logger.LogError("Trip \n({@Trip})\n at this Hub \n({@Hub})\n did not have an Appointment assigned.", trip, hub);

                continue;
            }
            
            var appointmentSlot = await _appointmentSlotRepository.GetAsync(appointment, cancellationToken);
            if (appointmentSlot == null)
            {
                _logger.LogError("Appointment \n({@Appointment})\n at this Hub \n({@Hub})\n did not have an AppointmentSlot assigned.", appointment, hub);

                continue;
            }

            if (nextTrip != null && appointmentSlot.StartTime > earliestAppointmentTime) continue;
            
            _logger.LogDebug("Trip \n({@Trip})\n is now has the earliest Appointment Time ({Step})\n for its Appointment \n({@Appointment})\n assigned to this AppointmentSlot \n({@AppointmentSlot})", trip, earliestAppointmentTime, appointment, appointmentSlot);
                        
            nextTrip = trip;
            earliestAppointmentTime = appointmentSlot.StartTime;
        }
        
        _logger.LogInformation("Trip \n({@Trip})\n has the earliest Appointment Time ({Step})\n for its Work with WorkType \n({WorkType})", nextTrip, earliestAppointmentTime, workType);
        return nextTrip;
    }

    public Task<Trip?> GetNextAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        return !_modelState.ModelConfig.AppointmentSystemMode ? 
            GetNextBaseAsync(hub, workType, cancellationToken) : 
            GetNextAppointmentAsync(hub, workType, cancellationToken);
    }
    
    public async Task<Trip?> GetNextAsync(Hub hub, Bay bay, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("Getting next Trip failed due to this function not being available without AppointmentSystemMode.");

            return null;
        }
        
        var trips = (_tripRepository.GetCurrent(hub, WorkType.WaitBay, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            var appointment = await _appointmentRepository.GetAsync(trip, cancellationToken);
            if (appointment == null)
            {
                _logger.LogError("Trip \n({@Trip})\n at this Hub \n({@Hub})\n did not have an Appointment assigned.", trip, hub);

                continue;
            }

            if (appointment.BayId != bay.Id)
            {
                _logger.LogDebug("Appointment \n({@Appointment})\n at this Hub \n({@Hub})\n is not assigned to this Bay \n({@Bay})\n, checking next.", appointment, hub, bay);
                
                continue;
            }
            
            var appointmentSlot = await _appointmentSlotRepository.GetAsync(appointment, cancellationToken);
            if (appointmentSlot == null)
            {
                _logger.LogError("Appointment \n({@Appointment})\n at this Hub \n({@Hub})\n did not have an AppointmentSlot assigned.", appointment, hub);

                continue;
            }

            if (appointmentSlot.StartTime > _modelState.ModelTime)
            {
                _logger.LogError("Trip \n({@Trip})\n with this Appointment \n({@Appointment})\n in AppointmentSlot \n({@AppointmentSlot})\n for this Bay \n({@Bay})\n is too early in this Step ({Step})", trip, appointment, appointmentSlot, bay, _modelState.ModelTime);

                continue;
            }
            
            _logger.LogInformation("Trip \n({@Trip})\n with this Appointment \n({@Appointment})\n in AppointmentSlot \n({@AppointmentSlot})\n for this Bay \n({@Bay})\n is active in this Step ({Step}).", trip, appointment, appointmentSlot, bay, _modelState.ModelTime);
            return trip;
        }
        
        _logger.LogInformation("Hub \n({@Hub})\n has no active Appointment assigned to this \n({@Bay})\n in this Step ({Step})", hub, bay, _modelState.ModelTime);
        return null;
    }
    
    public async Task AlertFreeAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work != null)
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned and can therefore not claim this Truck \n({@Truck})", trip, work, truck);
            
            return;
        }
        
        _logger.LogDebug("Setting this Truck \n({@Truck})\n to this Trip \n({@Trip})", truck, trip);
        await _tripRepository.SetAsync(trip, truck, cancellationToken);

        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.TravelHub, trip);
        }
        else
        {
            _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.WaitTravelHub, trip);
        }
        await _workFactory.GetNewObjectAsync(trip, cancellationToken);
        
        _logger.LogInformation("Truck \n({@Truck})\n successfully linked to this Trip \n({@Trip})", truck, trip);
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitParking })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}and can therefore not claim this ParkingSpot \n({@ParkingSpot})", trip, work, WorkType.WaitParking, parkingSpot);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Setting this ParkingSpot \n({@ParkingSpot})\n to this Trip \n({@Trip})", parkingSpot, trip);
        await _tripRepository.SetAsync(trip, parkingSpot, cancellationToken);
        
        _logger.LogDebug("Setting ParkingSpot \n({@ParkingSpot})\n location to this Trip \n({@Trip})", parkingSpot, trip);
        await _locationFactory.SetAsync(trip, parkingSpot, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.WaitCheckIn, trip);
        await _workFactory.GetNewObjectAsync(trip, WorkType.WaitCheckIn, cancellationToken);
        
        _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully linked to this Trip \n({@Trip})", parkingSpot, trip);
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitCheckIn })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType}and can therefore not claim this AdminStaff \n({@AdminStaff})", trip, work, WorkType.WaitCheckIn, adminStaff);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Setting this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})", adminStaff, trip);
        await _tripRepository.SetAsync(trip, adminStaff, cancellationToken);
        
        _logger.LogDebug("Adding Work for this AdminStaff \n({@AdminStaff})\n to this Trip \n({@Trip})", adminStaff, trip);
        await _workFactory.GetNewObjectAsync(trip, adminStaff, cancellationToken);
        
        _logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully linked to this Trip \n({@Trip})", adminStaff, trip);
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitBay })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not claim this Bay \n({@Bay})", trip, work, WorkType.WaitBay, bay);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);

        var parkingSpot = await _parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (parkingSpot == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no ParkingSpot assigned to complete its Wait for a Bay at.", trip);
        }
        else
        {
            _logger.LogDebug("Removing ParkingSpot \n({@ParkingSpot})\n from this Trip \n({@Trip})", parkingSpot, trip);
            await _tripRepository.UnsetAsync(trip, parkingSpot, cancellationToken);
            _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n successfully removed from this Trip \n({@Trip})", parkingSpot, trip);
        }
        
        _logger.LogDebug("Setting this Bay \n({@Bay})\n to this Trip \n({@Trip})", bay, trip);
        await _tripRepository.SetAsync(trip, bay, cancellationToken);
        
        _logger.LogDebug("Setting Bay \n({@Bay})\n location to this Trip \n({@Trip})", bay, trip);
        await _locationFactory.SetAsync(trip, bay, cancellationToken);
        
        _logger.LogDebug("Adding Work for this Bay \n({@Bay})\n to this Trip \n({@Trip})", bay, trip);
        await _workFactory.GetNewObjectAsync(trip, bay, cancellationToken);
        
        _logger.LogInformation("Bay \n({@Bay})\n successfully linked to this Trip \n({@Trip})", bay, trip);
    }

    public async Task AlertWaitTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.WaitTravelHub })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not be alerted to have its wait to travel to the Hub completed.", trip, work, WorkType.WaitTravelHub);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.TravelHub, trip);
        await _workFactory.GetNewObjectAsync(trip, WorkType.TravelHub, cancellationToken);
        
        _logger.LogInformation("Trip \n({@Trip})\n successfully waited to travel to the Hub.", trip);
    }
    
    public async Task AlertTravelHubCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHub })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not be alerted to have its travel to the Hub completed.", trip, work, WorkType.TravelHub);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.WaitParking, trip);
        await _workFactory.GetNewObjectAsync(trip, WorkType.WaitParking, cancellationToken);
        
        _logger.LogInformation("Trip \n({@Trip})\n successfully arrived at the Hub.", trip);
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.CheckIn })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not be alerted to have its Check-In completed.", trip, work, WorkType.CheckIn);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);

        var adminStaff = await _adminStaffRepository.GetAsync(trip, cancellationToken);
        if (adminStaff == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no AdminStaff assigned to complete its Check-In at...", trip);
        }
        else
        {
            _logger.LogDebug("Removing AdminStaff \n({@AdminStaff})\n from this Trip \n({@Trip})", adminStaff, trip);
            await _tripRepository.UnsetAsync(trip, adminStaff, cancellationToken);
            _logger.LogInformation("AdminStaff \n({@AdminStaff})\n successfully removed from this Trip \n({@Trip})", adminStaff, trip);
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.WaitBay, trip);
        await _workFactory.GetNewObjectAsync(trip, WorkType.WaitBay, cancellationToken);
        
        _logger.LogInformation("Trip \n({@Trip})\n successfully completed Check-In.", trip);
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.Bay })
        {
            _logger.LogError("Trip \n({@Trip})\n has active Work \n({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not be alerted to have its Bay Work completed.", trip, work, WorkType.Bay);
            
            return;
        }

        _logger.LogDebug("Removing active Work \n({@Work})\n assigned to this Trip \n({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip \n({@Trip})\n has no Bay assigned to complete its Bay Work at...", trip);
        }
        else
        {
            _logger.LogDebug("Removing Bay \n({@Bay})\n from this Trip \n({@Trip})", bay, trip);
            await _tripRepository.UnsetAsync(trip, bay, cancellationToken);
            _logger.LogInformation("Bay \n({@Bay})\n successfully removed from this Trip \n({@Trip})", bay, trip);
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} to this Trip \n({@Trip})", WorkType.TravelHome, trip);
        await _workFactory.GetNewObjectAsync(trip, WorkType.TravelHome, cancellationToken);
        
        _logger.LogInformation("Trip ({@Trip})\n successfully completed Bay Work.", trip);
    }
    
    public async Task AlertTravelHomeCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work is not { WorkType: WorkType.TravelHome })
        {
            _logger.LogError("Trip ({@Trip})\n has active Work ({@Work})\n assigned with WorkType not of type {@WorkType} and can therefore not be alerted to have its Travel Home completed.", trip, work, WorkType.TravelHome);
            
            return;
        }
        
        _logger.LogDebug("Removing active Work ({@Work})\n assigned to this Trip ({@Trip})", work, trip);
        await _workRepository.RemoveAsync(work, cancellationToken);
        
        await CompleteTripAsync(trip, cancellationToken);
    }

    private async Task CompleteTripAsync(Trip trip, CancellationToken cancellationToken)
    {
        await _tripRepository.SetAsync(trip, true, cancellationToken);
        await _palletService.CompleteAsync(trip, cancellationToken);

        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to complete with...", trip);
        }
        else
        {
            _logger.LogDebug("Removing Truck ({@Truck})\n from this Trip ({@Trip})", truck, trip);
            await _tripRepository.UnsetAsync(trip, truck, cancellationToken);
            _logger.LogInformation("Truck ({@Truck})\n successfully removed from this Trip ({@Trip})", truck, trip);
        }
        
        _instrumentation.Add(Metric.TripComplete, 1, ("Trip", trip.Id));
        
        _logger.LogInformation("Trip ({@Trip})\n successfully COMPLETED!!!", trip);
    }

    public async Task TravelAsync(Trip trip, Truck truck, long xDestination, long yDestination, CancellationToken cancellationToken)
    {
        var xDiff = xDestination - trip.XLocation;
        var xTravel = Math.Clamp(xDiff, -truck.Speed, truck.Speed);
        var newXLocation = trip.XLocation + xTravel;
        
        var yDiff = yDestination - trip.YLocation;
        var yTravel = Math.Clamp(yDiff, -truck.Speed, truck.Speed);
        var newYLocation = trip.YLocation + yTravel;

        _logger.LogDebug("Setting new location (XLocation={XLocation} YLocation={YLocation})\n to this Trip ({@Trip})", newXLocation, newYLocation, trip);
        await _locationFactory.SetAsync(trip, newXLocation, newYLocation, cancellationToken);
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
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel to the Hub with.", trip);
            
            return;
        }
        
        var hub = await _hubRepository.GetAsync(trip, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.", trip);
            
            return;
        }
        
        await TravelAsync(trip, truck, hub, cancellationToken);
    }

    public async Task TravelHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.", trip);
            
            return;
        }
        
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...", trip);
            
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
        
        _logger.LogError("Trip ({@Trip})\n has no Hub assigned to travel to.", trip);
        
        return false;

    }

    public async Task<bool> IsAtHomeAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip ({@Trip})\n has no Truck assigned to travel home with.", trip);
            
            return false;
        }
        
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany != null)
            return truckCompany.XLocation == trip.XLocation &&
                   truckCompany.YLocation == trip.YLocation;
        
        _logger.LogError("Trip ({@Trip})\n has no TruckCompany assigned to travel home to, poor thing...", trip);
            
        return false;

    }
}
