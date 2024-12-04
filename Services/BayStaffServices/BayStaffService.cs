using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;

namespace Services.BayStaffServices;

public sealed class BayStaffService
{
    private readonly ILogger<BayStaffService> _logger;
    private readonly ModelState _modelState;
    private readonly HubService _hubService;
    private readonly BayRepository _bayRepository;
    private readonly BayService _bayService;
    private readonly WorkService _workService;
    private readonly TripRepository _tripRepository;
    private readonly LoadRepository _loadRepository;
    private readonly BayShiftService _bayShiftService;
    private readonly BayStaffRepository _bayStaffRepository;
    private readonly Counter<int> _pickUpMissCounter;
    private readonly Counter<int> _fetchMissCounter;
    
    public BayStaffService(
        ILogger<BayStaffService> logger,
        ModelState modelState,
        HubService hubService,
        BayRepository bayRepository,
        BayService bayService,
        WorkService workService,
        TripRepository tripRepository,
        LoadRepository loadRepository,
        BayShiftService bayShiftService,
        BayStaffRepository bayStaffRepository,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _hubService = hubService;
        _bayRepository = bayRepository;
        _bayService = bayService;
        _workService = workService;
        _tripRepository = tripRepository;
        _loadRepository = loadRepository;
        _bayShiftService = bayShiftService;
        _bayStaffRepository = bayStaffRepository;

        _pickUpMissCounter = meter.CreateCounter<int>("pick-up-miss", "PickUpMiss", "#PickUp Loads Missed.");
        _fetchMissCounter = meter.CreateCounter<int>("fetch-miss", "FetchMiss", "#PickUp Load not fetched yet.");
    }
    
    public async Task<BayStaff?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await _hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            _logger.LogError("No Hub could be selected for the new BayStaff");

            return null;
        }
        _logger.LogDebug("Hub \n({@Hub})\n was selected for the new BayStaff.",
            hub);
        
        var bayStaff = new BayStaff
        {
            Hub = hub,
            WorkChance = _modelState.AgentConfig.BayStaffAverageWorkDays,
            AverageShiftLength = _modelState.AgentConfig.BayShiftAverageLength
        };

        await _bayStaffRepository.AddAsync(bayStaff, cancellationToken);
        
        _logger.LogDebug("Setting BayShifts for this BayStaff \n({@BayStaff})",
            bayStaff);
        await _bayShiftService.GetNewObjectsAsync(bayStaff, cancellationToken);

        return bayStaff;
    }
    
    public async Task AlertWorkCompleteAsync(WorkType workType, Bay bay, CancellationToken cancellationToken)
    {
        switch (workType)
        {
            case WorkType.DropOff when
                bay.BayStatus is 
                    not BayStatus.WaitingFetchStart and
                    not BayStatus.WaitingFetch and  // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.PickUpStarted:
                
                _logger.LogInformation("Bay \n({@Bay})\n just completed Work of type {WorkType} in this Step \n({Step})",
                    bay,
                    WorkType.DropOff,
                    _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Drop-Off Completed to this Bay \n({@Bay})",
                    bay);
                await _bayService.AlertDroppedOffAsync(bay, cancellationToken);
                
                break;
            
            case WorkType.Fetch:
                
                _logger.LogInformation("Bay \n({@Bay})\n just completed Work of type {WorkType} in this Step \n({Step})",
                    bay,
                    WorkType.Fetch,
                    _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Fetch Completed to this Bay \n({@Bay})",
                    bay);
                await _bayService.AlertFetchedAsync(bay, cancellationToken);
                
                break;
            
            case WorkType.PickUp when
                bay.BayStatus is
                    not BayStatus.Free and          // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.Claimed and
                    not BayStatus.DroppingOffStarted:
                
                _logger.LogInformation("Bay \n({@Bay})\n just completed Work of type {WorkType} in this Step \n({Step})",
                    bay,
                    WorkType.PickUp,
                    _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Pick-Up Completed to this Bay \n({@Bay})",
                    bay);
                await _bayService.AlertPickedUpAsync(bay, cancellationToken);
                
                break;
        }
    }
    
    public async Task AlertFreeAsync(BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.Closed)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore be opened in this Step \n({Step})",
                bayStaff,
                bay,
                BayStatus.Closed,
                _modelState.ModelTime);
            
            _logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                BayStatus.Free,
                bay);
            await _bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.Free)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore alert the Bay is free this Step \n({Step})",
                bayStaff,
                bay,
                BayStatus.Free,
                _modelState.ModelTime);
            
            _logger.LogDebug("Alerting Free to this Bay \n({@Bay})",
                bay);
            await _bayService.AlertFreeAsync(bay, cancellationToken);
        }
        
        if (bay.BayStatus == BayStatus.Claimed)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Drop-Off Work this Step \n({Step})",
                bayStaff,
                bay,
                BayStatus.Claimed,
                _modelState.ModelTime);
            
            _logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                BayStatus.DroppingOffStarted,
                bay);
            await _bayRepository.SetAsync(bay, BayStatus.DroppingOffStarted, cancellationToken);
            
            _logger.LogDebug("Starting Drop-Off Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
            
            return;
        }

        if (bay.BayStatus is
            BayStatus.DroppingOffStarted or
            BayStatus.WaitingFetchStart)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Fetch Work in this Step \n({Step})",
                bayStaff,
                bay,
                bay.BayStatus,
                _modelState.ModelTime);
            
            _logger.LogDebug("Starting Fetch Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            await StartFetchAsync(bay, bayStaff, cancellationToken);
            
            return;
        }

        if (bay.BayStatus is
            BayStatus.FetchStarted or
            BayStatus.FetchFinished)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Drop-Off Work in this Step \n({Step})",
                bayStaff,
                bay,
                bay.BayStatus,
                _modelState.ModelTime);
            
            _logger.LogDebug("Starting Drop-Off Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.PickUpStarted)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Pick-Up Work in this Step \n({Step})",
                bayStaff,
                bay,
                BayStatus.PickUpStarted,
                _modelState.ModelTime);
            
            _logger.LogDebug("Starting Pick-Up Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            await StartPickUpAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.WaitingFetch)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                  "and therefore has no work to be assigned in this Stef \n({Step})\n",
                bayStaff,
                bay,
                BayStatus.WaitingFetch,
                _modelState.ModelTime);
            
            _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle...",
                bay);
        }
    }
    
    public async Task StartDropOffAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to start Drop-Off Work for.",
                bay);
            
            return;
        }
        
        var dropOffLoad = await _loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (dropOffLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Drop-Off.",
                trip);
            
            _logger.LogInformation("Drop-Off Work could not be started for this Trip \n({@Trip})\n and is therefore completed.",
                trip);
        
            _logger.LogDebug("Alerting Drop-Off Work has completed for this Bay \n({@Bay})",
                bay);
            await _bayService.AlertDroppedOffAsync(bay, cancellationToken);
            
            _logger.LogDebug("Alerting Free for this BayStaff \n({@BayStaff})\n to this Bay \n({@Bay})",
                bayStaff,
                bay);
            await AlertFreeAsync(bayStaff, bay, cancellationToken);

            return;
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
            WorkType.DropOff,
            bayStaff,
            bay);
        await _workService.AddAsync(bay, bayStaff, WorkType.DropOff, cancellationToken);
        
        _logger.LogDebug("Adapting the Workload of other active Drop-Off Work for this Bay \n({@Bay})",
            bay);
        await _workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to start Fetch Work for.",
                bay);
            
            return;
        }
        
        var pickUpLoad = await _loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Pick-Up.",
                trip);
        }
        else
        {
            // TODO: BIG REFACTOR HERE
            Bay? pickUpLoadBay = null; // TODO: Refactor | await _bayRepository.GetAsync(pickUpLoad, cancellationToken);
            if (pickUpLoadBay == null)
            {
                _logger.LogInformation("Load \n({@Load})\n to Pick-Up for this Trip \n({@Trip})\n did not have a bay assigned to Fetch it from.",
                    pickUpLoad,
                    trip);

                if (bay.BayStatus == BayStatus.WaitingFetchStart)
                {
                    _logger.LogInformation("Bay \n({@Bay})\n has assigned BayStatus {@BayStatus}" +
                                          "and can therefore not wait longer to Fetch the Load \n({@Load})\n" +
                                          "for this Trip \n({@Trip})",
                        bay,
                        BayStatus.WaitingFetchStart,
                        pickUpLoad,
                        trip);
                    
                    _logger.LogDebug("Unsetting Pick-Up Load \n({@Load})\n for this Trip \n({@Trip})",
                        pickUpLoad,
                        trip);
                    // TODO: Refactor | await _loadRepository.UnsetPickUpAsync(pickUpLoad, trip, cancellationToken);
                    
                    _pickUpMissCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
                }
                else
                {
                    _logger.LogInformation("Bay \n({@Bay})\n has assigned BayStatus {@BayStatus}" +
                                          "and can therefore wait longer to Fetch the Load \n({@Load})\n" +
                                          "for this Trip \n({@Trip})",
                        bay,
                        bay.BayStatus,
                        pickUpLoad,
                        trip);
                    
                    _logger.LogDebug("Starting Drop-Off Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                        bayStaff,
                        bay);
                    await StartDropOffAsync(bay, bayStaff, cancellationToken);

                    return;
                }
            }
            else if (pickUpLoadBay.Id != bay.Id)
            {
                _logger.LogInformation("Load \n({@Load})\n to Pick-Up for this Trip \n({@Trip})\n is not assigned to the same Bay \n({@Bay})\n as this Bay \n({@Bay})",
                    pickUpLoad,
                    trip,
                    pickUpLoadBay,
                    bay);
                
                _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                    WorkType.Fetch,
                    bayStaff,
                    bay);
                await _workService.AddAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
                
                _fetchMissCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
                
                return;
            }
            else
            {
                _logger.LogInformation("Load \n({@Load})\n to Pick-Up for this Trip \n({@Trip})\n is assigned to the same Bay \n({@Bay})\n as this Bay \n({@Bay})",
                    pickUpLoad,
                    trip,
                    pickUpLoadBay,
                    bay);
            }
        }
            
        _logger.LogInformation("Fetch Work could not be started for this Trip \n({@Trip})\n and is therefore completed.",
            trip);
        
        _logger.LogDebug("Alerting Fetch Work has completed for this Bay \n({@Bay})",
            bay);
        await _bayService.AlertFetchedAsync(bay, cancellationToken);
            
        _logger.LogDebug("Alerting Free for this BayStaff \n({@BayStaff})\n to this Bay \n({@Bay})",
            bayStaff,
            bay);
        await AlertFreeAsync(bayStaff, bay, cancellationToken);
    }

    public async Task StartPickUpAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to start Pick-Up Work for.",
                bay);
            
            return;
        }
        
        var pickUpLoad = await _loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Pick-Up.",
                trip);
            
            _logger.LogInformation("Pick-Up Work could not be started for this Trip \n({@Trip})\n and is therefore completed.",
                trip);
        
            _logger.LogDebug("Alerting Pick-Up Work has completed for this Bay \n({@Bay})",
                bay);
            await _bayService.AlertPickedUpAsync(bay, cancellationToken);
            
            _logger.LogDebug("Alerting Free for this BayStaff \n({@BayStaff})\n to this Bay \n({@Bay})",
                bayStaff,
                bay);
            await AlertFreeAsync(bayStaff, bay, cancellationToken);

            return;
        }
        
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
            WorkType.PickUp,
            bayStaff,
            bay);
        await _workService.AddAsync(bay, bayStaff, WorkType.PickUp, cancellationToken);
        
        _logger.LogDebug("Adapting the Workload of other active Pick-Up Work for this Bay \n({@Bay})",
            bay);
        await _workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
}
