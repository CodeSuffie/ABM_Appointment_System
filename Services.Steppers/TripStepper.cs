using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class TripStepper : IStepperService<Trip>
{
    private readonly ILogger<TripStepper> _logger;
    private readonly TripRepository _tripRepository;
    private readonly WorkRepository _workRepository;
    private readonly WorkService _workService;
    private readonly TripService _tripService;
    private readonly ModelState _modelState;
    
    private readonly Histogram<int> _unclaimedTripsHistogram;
    private readonly Histogram<int> _claimedTripsHistogram;
    private readonly Histogram<int> _waitTravelHubTripsHistogram;
    private readonly Histogram<int> _travelHubTripsHistogram;
    private readonly Histogram<int> _arrivedTripsHistogram;
    private readonly Histogram<int> _parkedTripsHistogram;
    private readonly Histogram<int> _checkingInTripsHistogram;
    private readonly Histogram<int> _checkInCompleteTripsHistogram;
    private readonly Histogram<int> _atBayTripsHistogram;
    private readonly Histogram<int> _travelHomeTripsHistogram;
    private readonly Histogram<int> _completedTripsHistogram;

    public TripStepper(ILogger<TripStepper> logger,
        TripRepository tripRepository,
        WorkRepository workRepository,
        WorkService workService,
        TripService tripService,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _workService = workService;
        _tripService = tripService;
        _modelState = modelState;
        
        _unclaimedTripsHistogram = meter.CreateHistogram<int>("unclaimed-trip", "Trip", "#Trips Unclaimed (excl. Completed).");
        _claimedTripsHistogram = meter.CreateHistogram<int>("claimed-trip", "Trip", "#Trips Claimed (excl. Completed).");
        _waitTravelHubTripsHistogram = meter.CreateHistogram<int>("wait-travel-hub-trip", "Trip", "#Trips Waiting to Travel to the Hub.");
        _travelHubTripsHistogram = meter.CreateHistogram<int>("travel-hub-trip", "Trip", "#Trips Travelling to the Hub.");
        _arrivedTripsHistogram = meter.CreateHistogram<int>("arrived-hub-trip", "Trip", "#Trips Arrived at the Hub but not yet parking.");
        _parkedTripsHistogram = meter.CreateHistogram<int>("parking-trip", "Trip", "#Trips Parking but not yet Checked-In.");
        _checkingInTripsHistogram = meter.CreateHistogram<int>("checking-in-trip", "Trip", "#Trips Currently Checking In.");
        _checkInCompleteTripsHistogram = meter.CreateHistogram<int>("checked-in-trip", "Trip", "#Trips Checked In but not yet at a Bay.");
        _atBayTripsHistogram = meter.CreateHistogram<int>("bay-trip", "Trip", "#Trips Currently at a Bay.");
        _travelHomeTripsHistogram = meter.CreateHistogram<int>("travel-home-trip", "Trip", "#Trips with completed Bay Work and Travelling home.");
        _completedTripsHistogram = meter.CreateHistogram<int>("completed-trip", "Trip", "#Trips Completed.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Trip in this Step ({Step})", _modelState.ModelTime);

        // var unclaimed = await _tripRepository.CountAsync(false, cancellationToken);
        // _unclaimedTripsHistogram.Record(unclaimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var claimed = await _tripRepository.CountAsync(true, cancellationToken);
        // _claimedTripsHistogram.Record(claimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var waitTravelHub = await _tripRepository.CountAsync(WorkType.WaitTravelHub, cancellationToken);
        // _waitTravelHubTripsHistogram.Record(waitTravelHub, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var travelHub = await _tripRepository.CountAsync(WorkType.TravelHub, cancellationToken);
        // _travelHubTripsHistogram.Record(travelHub, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var arrived = await _tripRepository.CountAsync(WorkType.WaitParking, cancellationToken);
        // _arrivedTripsHistogram.Record(arrived, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var parked = await _tripRepository.CountAsync(WorkType.WaitCheckIn, cancellationToken);
        // _parkedTripsHistogram.Record(parked, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var checkingIn = await _tripRepository.CountAsync(WorkType.CheckIn, cancellationToken);
        // _checkingInTripsHistogram.Record(checkingIn, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var checkInComplete = await _tripRepository.CountAsync(WorkType.WaitBay, cancellationToken);
        // _checkInCompleteTripsHistogram.Record(checkInComplete, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var atBay = await _tripRepository.CountAsync(WorkType.Bay, cancellationToken);
        // _atBayTripsHistogram.Record(atBay, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var travelHome = await _tripRepository.CountAsync(WorkType.TravelHome, cancellationToken);
        // _travelHomeTripsHistogram.Record(travelHome, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var completed = await _tripRepository.CountCompletedAsync(cancellationToken);
        // _completedTripsHistogram.Record(completed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Trip in this Step ({Step})", _modelState.ModelTime);
    }
    
    public async Task StepAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(trip, cancellationToken);
        if (work == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n has no active Work assigned in this Step ({Step})", trip, _modelState.ModelTime);
            
            _logger.LogDebug("Trip \n({@Trip})\n will remain idle in this Step ({Step})", trip, _modelState.ModelTime);
            
            return;
        }

        if (work.WorkType == WorkType.WaitTravelHub)
        {
            if (_workService.IsWorkCompleted(work))
            {
                _logger.LogInformation("Trip \n({@Trip})\n just completed assigned Work \n({@Work})\n in this Step ({Step})", trip, work, _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Wait for Travel Hub Completed for this Trip \n({@Trip})\n in this Step ({Step})", trip, _modelState.ModelTime);
                await _tripService.AlertWaitTravelHubCompleteAsync(trip, cancellationToken);
            }
        }

        else if (work.WorkType == WorkType.TravelHub)
        {
            _logger.LogInformation("Trip \n({@Trip})\n has Work \n({@Work})\n assigned of Type {WorkType} in this Step ({Step})", trip, work, WorkType.TravelHub, _modelState.ModelTime);
            
            if (await _tripService.IsAtHubAsync(trip, cancellationToken))
            {
                _logger.LogInformation("Trip \n({@Trip})\n has arrived at the Hub in this Step ({Step})", trip, _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Travel to Hub Complete for this Trip \n({@Trip})\n in this Step ({Step})", trip, _modelState.ModelTime);
                await _tripService.AlertTravelHubCompleteAsync(trip, cancellationToken);
            }
            
            _logger.LogDebug("Travelling to the Hub for this Trip \n({@Trip})\n in this Step ({Step})", trip, _modelState.ModelTime);
            await _tripService.TravelHubAsync(trip, cancellationToken);
        }

        else if (work.WorkType == WorkType.TravelHome)
        {
            _logger.LogInformation("Trip \n({@Trip})\n has Work \n({@Work})\n assigned of Type {WorkType} in this Step ({Step})", trip, work, WorkType.TravelHome, _modelState.ModelTime);
            
            _logger.LogDebug("Travelling home for this Trip \n({@Trip})\n in this Step ({Step})", trip, _modelState.ModelTime);
            await _tripService.TravelHomeAsync(trip, cancellationToken);
            
            if (await _tripService.IsAtHomeAsync(trip, cancellationToken))
            {
                _logger.LogInformation("Trip \n({@Trip})\n has arrived home in this Step ({Step})", trip, _modelState.ModelTime);
                
                _logger.LogDebug("Alerting Travel home Complete for this Trip \n({@Trip})\n in this Step ({Step})", trip, _modelState.ModelTime);
                await _tripService.AlertTravelHomeCompleteAsync(trip, cancellationToken);
            }
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trips = _tripRepository.Get(true)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var trip in trips)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this Trip \n({@Trip})", _modelState.ModelTime, trip);
            
            await StepAsync(trip, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this Trip \n({@Trip})", _modelState.ModelTime, trip);
        }
    }
}