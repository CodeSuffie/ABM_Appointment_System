using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;
using Services.PelletServices;
using SQLitePCL;

namespace Services.BayStaffServices;

public sealed class BayStaffService
{
    private readonly ILogger<BayStaffService> _logger;
    private readonly ModelState _modelState;
    private readonly PelletService _pelletService;
    private readonly PelletRepository _pelletRepository;
    private readonly HubService _hubService;
    private readonly BayRepository _bayRepository;
    private readonly BayService _bayService;
    private readonly WorkService _workService;
    private readonly TripRepository _tripRepository;
    private readonly BayShiftService _bayShiftService;
    private readonly BayStaffRepository _bayStaffRepository;
    private readonly Counter<int> _pickUpMissCounter;
    private readonly Counter<int> _fetchMissCounter;
    
    public BayStaffService(
        ILogger<BayStaffService> logger,
        ModelState modelState,
        PelletService pelletService,
        PelletRepository pelletRepository,
        HubService hubService,
        BayRepository bayRepository,
        BayService bayService,
        WorkService workService,
        TripRepository tripRepository,
        BayShiftService bayShiftService,
        BayStaffRepository bayStaffRepository,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _pelletService = pelletService;
        _pelletRepository = pelletRepository;
        _hubService = hubService;
        _bayRepository = bayRepository;
        _bayService = bayService;
        _workService = workService;
        _tripRepository = tripRepository;
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
    
    public async Task AlertWorkCompleteAsync(Work work, Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        
        if (trip == null)
        {
            if (bay.BayStatus != BayStatus.Closed && bay.BayStatus != BayStatus.Free)
            {
                _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to alert Work complete for.",
                    bay);

                _logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.Free,
                    bay);
                await _bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
            }

            return;
        }

        var pellet = await _pelletRepository.GetAsync(work, cancellationToken);

        if (pellet == null)
        {
            _logger.LogError("Work \n({@Work})\n did not have a Pellet assigned to complete Work for.",
                work);

            //_logger.LogDebug("Removing invalid Work {@Work} for this Bay \n({@Bay})",
            //    work,
            //    bay);
            //await _workRepository.RemoveAsync(work, cancellationToken);

            return;
        }
        
        switch (work.WorkType)
        {
            case WorkType.DropOff:
                await _pelletService.AlertDroppedOffAsync(pellet, bay, cancellationToken);
                break;
                
                
            case WorkType.Fetch:
                await _pelletService.AlertFetchedAsync(pellet, bay, cancellationToken);
                break;
            
            case WorkType.PickUp:
                await _pelletService.AlertPickedUpAsync(pellet, trip, cancellationToken);
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

        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        
        if (trip == null)
        {
            if (bay.BayStatus != BayStatus.Closed && bay.BayStatus != BayStatus.Free)
            {
                _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to start Drop-Off Work for.",
                    bay);

                _logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})",
                    BayStatus.Free,
                    bay);
                await _bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);

                await AlertFreeAsync(bayStaff, bay, cancellationToken);
            }

            return;
        }

        if (bay.BayStatus == BayStatus.Claimed)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus}" +
                                   "and can therefore try to start Drop-Off Work this Step \n({Step})",
                bayStaff,
                bay,
                BayStatus.Claimed,
                _modelState.ModelTime);
            
            _logger.LogDebug("Try to start Drop-Off Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            var startedDropOff = await TryStartDropOffAsync(trip, bayStaff, bay, cancellationToken);

            if (!startedDropOff)
            {
                var startedFetch = await TryStartFetchAsync(trip, bayStaff, bay, cancellationToken);
                if (!startedFetch)
                {
                    await TryStartPickUpAsync(trip, bayStaff, bay, cancellationToken);
                }
            }
        }
        
        if (bay.BayFlags.HasFlag(BayFlags.DroppedOff))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayFlag {@BayFlag}" +
                                  "and can therefore try to start Fetch Work in this Step \n({Step})",
                bayStaff,
                bay,
                BayFlags.DroppedOff,
                _modelState.ModelTime);
            
            _logger.LogDebug("Try to start Fetch Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            var startedFetch = await TryStartFetchAsync(trip, bayStaff, bay, cancellationToken);
            if (!startedFetch)
            {
                await TryStartPickUpAsync(trip, bayStaff, bay, cancellationToken);
            }
        }

        if (bay.BayFlags.HasFlag(BayFlags.Fetched))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayFlag {@BayFlag}" +
                                  "and can therefore try to start Fetch Work in this Step \n({Step})",
                bayStaff,
                bay,
                BayFlags.Fetched,
                _modelState.ModelTime);
            
            _logger.LogDebug("Try to start Pick-Up Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
                bayStaff,
                bay);
            await TryStartPickUpAsync(trip, bayStaff, bay, cancellationToken);
        }
    }

    private async Task<bool> TryStartDropOffAsync(Trip trip, BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextDropOffAsync(trip, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have any more Pellets assigned to Drop-Off.",
                trip);
            
            _logger.LogInformation("Drop-Off Work could not be started for this Trip \n({@Trip}).",
                trip);
                
            return false;
        }

        _logger.LogDebug("Starting Drop-Off Work for this BayStaff \n({@BayStaff})\n for this Trip \n({@Trip})\n for this Pellet \n({@Pellet})",
            bayStaff,
            trip,
            pellet);
        await StartDropOffAsync(bayStaff, pellet, bay, cancellationToken);

        return true;
    }

    private async Task<bool> TryStartFetchAsync(Trip trip, BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextFetchAsync(trip, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have any more Pellets assigned to Fetch.",
                trip);
            
            _logger.LogInformation("Fetch Work could not be started for this Trip \n({@Trip}).",
                trip);
            
            return false;
        }
            
        _logger.LogDebug("Starting Fetch Work for this BayStaff \n({@BayStaff})\n for this Trip \n({@Trip})\n for this Pellet \n({@Pellet})",
            bayStaff,
            trip,
            pellet);
        await StartFetchAsync(bayStaff, pellet, bay, cancellationToken);
            
        return true;
    }
    
    private async Task<bool> TryStartPickUpAsync(Trip trip, BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextPickUpAsync(trip, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have any more Pellets assigned to Pick-Up.",
                trip);
            
            _logger.LogInformation("Pick-Up Work could not be started for this Trip \n({@Trip}).",
                trip);
                
            return false;
        }
            
        _logger.LogDebug("Starting Pick-Up Work for this BayStaff \n({@BayStaff})\n for this Trip \n({@Trip})\n for this Pellet \n({@Pellet})",
            bayStaff,
            trip,
            pellet);
        await StartPickUpAsync(bayStaff, pellet, bay, cancellationToken);

        return true;
    }

    public async Task StartDropOffAsync(BayStaff bayStaff, Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
            WorkType.DropOff,
            bayStaff,
            bay);
        await _workService.AddAsync(bay, bayStaff, pellet, WorkType.DropOff, cancellationToken);
    }
    
    public async Task StartFetchAsync(BayStaff bayStaff, Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
            WorkType.Fetch,
            bayStaff,
            bay);
        await _workService.AddAsync(bay, bayStaff, pellet, WorkType.Fetch, cancellationToken);
        
        _fetchMissCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
    }

    public async Task StartPickUpAsync(BayStaff bayStaff, Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})",
            WorkType.PickUp,
            bayStaff,
            bay);
        await _workService.AddAsync(bay, bayStaff, pellet, WorkType.PickUp, cancellationToken);
    }
}
