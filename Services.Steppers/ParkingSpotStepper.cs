using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class ParkingSpotStepper : IStepperService<ParkingSpot>
{
    private readonly ILogger<ParkingSpotStepper> _logger;
    private readonly ParkingSpotService _parkingSpotService;
    private readonly ParkingSpotRepository _parkingSpotRepository;
    private readonly TripRepository _tripRepository;
    private readonly ModelState _modelState;

    public ParkingSpotStepper(
        ILogger<ParkingSpotStepper> logger,
        ParkingSpotService parkingSpotService,
        ParkingSpotRepository parkingSpotRepository,
        TripRepository tripRepository,
        ModelState modelState)
    {
        _logger = logger;
        _parkingSpotService = parkingSpotService;
        _parkingSpotRepository = parkingSpotRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;
    }
    
    public async Task StepAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(parkingSpot, cancellationToken);

        if (trip != null)
        {
            _logger.LogDebug("ParkingSpot \n({@ParkingSpot})\n has an active Trip assigned in this Step ({Step})", parkingSpot, _modelState.ModelTime);

            _logger.LogDebug("ParkingSpot \n({@ParkingSpot})\n will remain idle in this Step ({Step})", parkingSpot, _modelState.ModelTime);

            return;
        }

        _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n has no active Trip assigned in this Step ({Step})", parkingSpot, _modelState.ModelTime);
        
        _logger.LogDebug("Alerting Free for this ParkingSpot \n({@ParkingSpot})\n in this Step ({Step})", parkingSpot, _modelState.ModelTime);
        await _parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var parkingSpots = (_parkingSpotRepository.Get())
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var parkingSpot in parkingSpots)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this ParkingSpot \n({@ParkingSpot})", _modelState.ModelTime, parkingSpot);
            
            await StepAsync(parkingSpot, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this ParkingSpot \n({@ParkingSpot})", _modelState.ModelTime, parkingSpot);
        }
    }
}