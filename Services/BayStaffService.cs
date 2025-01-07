using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;
using Settings;

namespace Services;

public sealed class BayStaffService
{
    private readonly ILogger<BayStaffService> _logger;
    private readonly ModelState _modelState;
    private readonly PelletService _pelletService;
    private readonly PelletRepository _pelletRepository;
    private readonly BayRepository _bayRepository;
    private readonly WorkFactory _workFactory;
    private readonly TripRepository _tripRepository;
    private readonly BayService _bayService;
    private readonly Instrumentation _instrumentation;

    public BayStaffService(
        ILogger<BayStaffService> logger,
        ModelState modelState,
        PelletService pelletService,
        PelletRepository pelletRepository,
        BayRepository bayRepository,
        WorkFactory workFactory,
        TripRepository tripRepository,
        BayService bayService, 
        Instrumentation instrumentation)
    {
        _logger = logger;
        _modelState = modelState;
        _pelletService = pelletService;
        _pelletRepository = pelletRepository;
        _bayRepository = bayRepository;
        _workFactory = workFactory;
        _tripRepository = tripRepository;
        _bayService = bayService;
        _instrumentation = instrumentation;
    }
    
    public async Task AlertWorkCompleteAsync(Work work, Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned to alert Work complete for.", bay);

            return;
        }

        var pellet = await _pelletRepository.GetAsync(work, cancellationToken);

        if (pellet == null)
        {
            _logger.LogError("Work \n({@Work})\n did not have a Pellet assigned to complete Work for.", work);

            return;
        }
        
        switch (work.WorkType)
        {
            case WorkType.DropOff:
                await _pelletService.AlertDroppedOffAsync(pellet, bay, cancellationToken);
                _instrumentation.DropOffBayStaffCounter.Add(-1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("BayStaff", bayStaff.Id),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("Pellet", pellet.Id)
                    ]);
                break;
            
            case WorkType.PickUp:
                await _pelletService.AlertPickedUpAsync(pellet, trip, cancellationToken);
                _instrumentation.PickUpBayStaffCounter.Add(-1, 
                [
                        new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                        new KeyValuePair<string, object?>("BayStaff", bayStaff.Id),
                        new KeyValuePair<string, object?>("Bay", bay.Id),
                        new KeyValuePair<string, object?>("Pellet", pellet.Id)
                    ]);
                break;
        }
    }
    
    public async Task AlertFreeAsync(BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.Closed)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayStatus {@BayStatus} and can therefore be opened in this Step ({Step})", bayStaff, bay, BayStatus.Closed, _modelState.ModelTime);
            
            _logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay \n({@Bay})", BayStatus.Opened, bay);
            await _bayRepository.SetAsync(bay, BayStatus.Opened, cancellationToken);
        }

        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned to start Work for.", bay);

            return;
        }
        
        _logger.LogDebug("Try to start Drop-Off Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})", bayStaff, bay);
        var startedDropOff = await TryStartDropOffAsync(trip, bayStaff, bay, cancellationToken);

        if (!startedDropOff)
        {
            _logger.LogDebug("Try to start Pick-Up Work for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})", bayStaff, bay);
            var startedPickUp = await TryStartPickUpAsync(trip, bayStaff, bay, cancellationToken);
            
            if (!startedPickUp)
            {
                _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with assigned BayFlag {@BayFlag}and can therefore remain idle in this Step ({Step})", bayStaff, bay, BayFlags.PickedUp, _modelState.ModelTime);
            }
        }
    }

    private async Task<bool> TryStartDropOffAsync(Trip trip, BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayFlags.HasFlag(BayFlags.DroppedOff))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with Drop-Off work completed and can therefore not start Drop-Off Work this Step ({Step})", bayStaff, bay, _modelState.ModelTime);

            return false;
        }

        if (!await _bayService.HasRoomForPelletAsync(bay, cancellationToken))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with no room for another Pellet and can therefore not start Drop-Off Work this Step ({Step})", bayStaff, bay, _modelState.ModelTime);
            
            _instrumentation.DropOffMissCounter.Add(1, 
            [
                    new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                    new KeyValuePair<string, object?>("BayStaff", bayStaff.Id),
                    new KeyValuePair<string, object?>("Bay", bay.Id),
                    new KeyValuePair<string, object?>("Trip", trip.Id)
                ]);

            return false;
        }

        var pellet = await _pelletService.GetNextDropOffAsync(bay, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have any more Pellets assigned to Drop-Off.", trip);
            
            _logger.LogInformation("Drop-Off Work could not be started for this Trip \n({@Trip}).", trip);
                
            return false;
        }

        _logger.LogDebug("Starting Drop-Off Work for this BayStaff \n({@BayStaff})\n for this Trip \n({@Trip})\n for this Pellet \n({@Pellet})", bayStaff, trip, pellet);
        await StartDropOffAsync(bayStaff, pellet, bay, cancellationToken);

        return true;
    }
    
    private async Task<bool> TryStartPickUpAsync(Trip trip, BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayFlags.HasFlag(BayFlags.PickedUp))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n is working at Bay \n({@Bay})\n with Pick-Up work completed and can therefore not start Pick-Up Work this Step ({Step})", bayStaff, bay, _modelState.ModelTime);

            return false;
        }
        
        var pellet = await _pelletService.GetNextPickUpAsync(bay, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have any more Pellets assigned to Pick-Up.", trip);
            
            _logger.LogInformation("Pick-Up Work could not be started for this Trip \n({@Trip}).", trip);
                
            return false;
        }
            
        _logger.LogDebug("Starting Pick-Up Work for this BayStaff \n({@BayStaff})\n for this Trip \n({@Trip})\n for this Pellet \n({@Pellet})", bayStaff, trip, pellet);
        await StartPickUpAsync(bayStaff, pellet, bay, cancellationToken);

        return true;
    }

    public async Task StartDropOffAsync(BayStaff bayStaff, Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})", WorkType.DropOff, bayStaff, bay);
        await _workFactory.GetNewObjectAsync(bay, bayStaff, pellet, WorkType.DropOff, cancellationToken);
        
        _instrumentation.DropOffBayStaffCounter.Add(1, 
        [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("BayStaff", bayStaff.Id),
                new KeyValuePair<string, object?>("Bay", bay.Id),
                new KeyValuePair<string, object?>("Pellet", pellet.Id)
            ]);
    }

    public async Task StartPickUpAsync(BayStaff bayStaff, Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adding Work of type {WorkType} for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})", WorkType.PickUp, bayStaff, bay);
        await _workFactory.GetNewObjectAsync(bay, bayStaff, pellet, WorkType.PickUp, cancellationToken);
        
        _instrumentation.PickUpBayStaffCounter.Add(1, 
        [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("BayStaff", bayStaff.Id),
                new KeyValuePair<string, object?>("Bay", bay.Id),
                new KeyValuePair<string, object?>("Pellet", pellet.Id)
            ]);
    }
}
