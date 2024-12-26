using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services;

public sealed class BayService
{
    private readonly ILogger<BayService> _logger;
    private readonly HubRepository _hubRepository;
    private readonly PelletRepository _pelletRepository;
    private readonly PelletService _pelletService;
    private readonly TripService _tripService;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly WorkRepository _workRepository;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _droppedOffBaysCounter;
    private readonly UpDownCounter<int> _fetchedBaysCounter;
    private readonly UpDownCounter<int> _pickedUpBaysCounter;

    public BayService(ILogger<BayService> logger,
        HubRepository hubRepository,
        PelletRepository pelletRepository,
        PelletService pelletService,
        TripService tripService,
        BayRepository bayRepository,
        TripRepository tripRepository,
        WorkRepository workRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _hubRepository = hubRepository;
        _pelletRepository = pelletRepository;
        _pelletService = pelletService;
        _tripService = tripService;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _modelState = modelState;
        
        _droppedOffBaysCounter = meter.CreateUpDownCounter<int>("dropped-off-bay", "Bay", "#Bays Finished Drop-Off.");
        _fetchedBaysCounter = meter.CreateUpDownCounter<int>("fetched-bay", "Bay", "#Bays Finished Fetching.");
        _pickedUpBaysCounter = meter.CreateUpDownCounter<int>("picking-up-bay", "Bay", "#Bays Working on a Pick-Up.");
    }

    public async Task AlertWorkCompleteAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) 
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to alert completed Work for",
                bay);

            return;
        }
        
        _logger.LogDebug("Alerting Bay Work Completed for this Bay \n({@Bay})\n to assigned Trip \n({@Trip})",
            bay,
            trip);
        await _tripService.AlertBayWorkCompleteAsync(trip, cancellationToken);
            
        var work = await _workRepository.GetAsync(bay, cancellationToken);
        if (work == null) return;
        
        _logger.LogDebug("Removing completed Work \n({@Work})\n for this Bay \n({@Bay})",
            work,
            bay);
        await _workRepository.RemoveAsync(work, cancellationToken);
    }

    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Hub assigned to alert free for.",
                bay);

            return;
        }

        var trip = !_modelState.ModelConfig.AppointmentSystemMode ?
            await _tripService.GetNextAsync(hub, WorkType.Bay, cancellationToken) :
            await _tripService.GetNextAsync(hub, bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Hub \n({@Hub})\n did not have a Trip for this Bay \n({@Bay})\n to assign Bay Work for.",
                hub,
                bay);
            
            _logger.LogDebug("Bay \n({@Bay})\n will remain idle...",
                bay);
            
            return;
        }

        _logger.LogDebug("Alerting Free for this Bay \n({@Bay})\n to selected Trip \n({@Trip})",
            bay,
            trip);
        await _tripService.AlertFreeAsync(trip, bay, cancellationToken);
    }
    
    public async Task UpdateFlagsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay ({@Bay}) did not have a Trip assigned.",
                bay);
            
            _logger.LogDebug("Removing all BayFlags from thisBay ({@Bay}).",
                bay);
            await _bayRepository.RemoveAsync(bay, BayFlags.DroppedOff | BayFlags.Fetched | BayFlags.PickedUp, cancellationToken);
            return;
        }

        if (! await _pelletService.HasDropOffPelletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.DroppedOff))
            {
                await _bayRepository.AddAsync(bay, BayFlags.DroppedOff, cancellationToken);
                _droppedOffBaysCounter.Add(1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.DroppedOff),
                    ]);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.DroppedOff))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.DroppedOff, cancellationToken);
                _droppedOffBaysCounter.Add(-1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.DroppedOff),
                    ]);
            }
        }
        
        if (! await _pelletService.HasFetchPelletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.Fetched))
            {
                await _bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
                _fetchedBaysCounter.Add(1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.Fetched),
                    ]);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.Fetched))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.Fetched, cancellationToken);
                _fetchedBaysCounter.Add(-1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.Fetched),
                    ]);
            }
        }
        
        if (! await _pelletService.HasPickUpPelletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.PickedUp))
            {
                await _bayRepository.AddAsync(bay, BayFlags.PickedUp, cancellationToken);
                _pickedUpBaysCounter.Add(1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.PickedUp),
                    ]);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.PickedUp))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.PickedUp, cancellationToken);
                _pickedUpBaysCounter.Add(-1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("BayFlag", BayFlags.PickedUp),
                    ]);
            }
        }
    }

    public async Task<bool> HasRoomForPelletAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pelletCount = await _pelletRepository.Get(bay)
            .CountAsync(cancellationToken);
        var dropOffWorkCount = await _workRepository.Get(bay, WorkType.DropOff)
            .CountAsync(cancellationToken);
        var fetchWorkCount = await _workRepository.Get(bay, WorkType.Fetch)
            .CountAsync(cancellationToken);

        return (pelletCount + dropOffWorkCount + fetchWorkCount) < bay.Capacity;
    }
}
