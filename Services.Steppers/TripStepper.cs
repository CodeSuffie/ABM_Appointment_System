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

    public TripStepper(ILogger<TripStepper> logger,
        TripRepository tripRepository,
        WorkRepository workRepository,
        WorkService workService,
        TripService tripService,
        ModelState modelState)
    {
        _logger = logger;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _workService = workService;
        _tripService = tripService;
        _modelState = modelState;
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