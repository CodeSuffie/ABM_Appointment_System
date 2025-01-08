using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayService
{
    private readonly ILogger<BayService> _logger;
    private readonly HubRepository _hubRepository;
    private readonly PalletRepository _palletRepository;
    private readonly PalletService _palletService;
    private readonly TripService _tripService;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly WorkRepository _workRepository;
    private readonly BayShiftService _bayShiftService;
    private readonly ModelState _modelState;

    public BayService(ILogger<BayService> logger,
        HubRepository hubRepository,
        PalletRepository palletRepository,
        PalletService palletService,
        TripService tripService,
        BayRepository bayRepository,
        TripRepository tripRepository,
        BayShiftService bayShiftService,
        WorkRepository workRepository,
        ModelState modelState)
    {
        _logger = logger;
        _hubRepository = hubRepository;
        _palletRepository = palletRepository;
        _palletService = palletService;
        _tripService = tripService;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _bayShiftService = bayShiftService;
        _workRepository = workRepository;
        _modelState = modelState;
    }

    public async Task AlertWorkCompleteAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null) 
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned to alert completed Work for", bay);

            return;
        }
        
        _logger.LogDebug("Alerting Bay Work Completed for this Bay \n({@Bay})\n to assigned Trip \n({@Trip})", bay, trip);
        await _tripService.AlertBayWorkCompleteAsync(trip, cancellationToken);
            
        var work = await _workRepository.GetAsync(bay, cancellationToken);
        if (work == null) return;
        
        _logger.LogDebug("Removing completed Work \n({@Work})\n for this Bay \n({@Bay})", work, bay);
        await _workRepository.RemoveAsync(work, cancellationToken);
    }

    public async Task AlertFreeAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Hub assigned to alert free for.", bay);

            return;
        }

        var trip = !_modelState.ModelConfig.AppointmentSystemMode ?
            await _tripService.GetNextAsync(hub, WorkType.WaitBay, cancellationToken) :
            await _tripService.GetNextAsync(hub, bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Hub \n({@Hub})\n did not have a Trip for this Bay \n({@Bay})\n to assign Bay Work for.", hub, bay);
            
            _logger.LogDebug("Bay \n({@Bay})\n will remain idle...", bay);
            
            return;
        }

        _logger.LogDebug("Alerting Free for this Bay \n({@Bay})\n to selected Trip \n({@Trip})", bay, trip);
        await _tripService.AlertFreeAsync(trip, bay, cancellationToken);
    }

    public async Task UpdateStatusAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = _bayShiftService.GetCurrent(bay, cancellationToken);
        var works = _workRepository.Get(bay);
        if (!await works.AnyAsync(cancellationToken) && !await shifts.AnyAsync(cancellationToken))
        {
            if (bay.BayStatus != BayStatus.Closed)
            {
                await _bayRepository.SetAsync(bay, BayStatus.Closed, cancellationToken);
            }
        }
        else if (bay.BayStatus != BayStatus.Opened)
        {
            await _bayRepository.SetAsync(bay, BayStatus.Opened, cancellationToken);
        }
    }
    
    public async Task UpdateFlagsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay ({@Bay}) did not have a Trip assigned.", bay);
            
            _logger.LogDebug("Removing all BayFlags from thisBay ({@Bay}).", bay);
            await _bayRepository.RemoveAsync(bay, BayFlags.DroppedOff | BayFlags.Fetched | BayFlags.PickedUp, cancellationToken);
            return;
        }

        if (! await _palletService.HasDropOffPalletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.DroppedOff))
            {
                await _bayRepository.AddAsync(bay, BayFlags.DroppedOff, cancellationToken);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.DroppedOff))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.DroppedOff, cancellationToken);
            }
        }
        
        if (! await _palletService.HasFetchPalletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.Fetched))
            {
                await _bayRepository.AddAsync(bay, BayFlags.Fetched, cancellationToken);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.Fetched))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.Fetched, cancellationToken);
            }
        }
        
        if (! await _palletService.HasPickUpPalletsAsync(bay, cancellationToken))
        {
            if (! bay.BayFlags.HasFlag(BayFlags.PickedUp))
            {
                await _bayRepository.AddAsync(bay, BayFlags.PickedUp, cancellationToken);
            }
        }
        else
        {
            if (bay.BayFlags.HasFlag(BayFlags.PickedUp))
            {
                await _bayRepository.RemoveAsync(bay, BayFlags.PickedUp, cancellationToken);
            }
        }
    }

    public async Task<bool> HasRoomForPalletAsync(Bay bay, CancellationToken cancellationToken)
    {
        var palletCount = await _palletRepository.Get(bay)
            .CountAsync(cancellationToken);
        var dropOffWorkCount = await _workRepository.Get(bay, WorkType.DropOff)
            .CountAsync(cancellationToken);
        var fetchWorkCount = await _workRepository.Get(bay, WorkType.Fetch)
            .CountAsync(cancellationToken);

        return (palletCount + dropOffWorkCount + fetchWorkCount) < bay.Capacity;
    }
}
