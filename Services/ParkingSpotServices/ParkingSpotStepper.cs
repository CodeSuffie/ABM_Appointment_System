using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotStepper : IStepperService<ParkingSpot>
{
    private readonly ILogger<ParkingSpotStepper> _logger;
    private readonly ParkingSpotService _parkingSpotService;
    private readonly ParkingSpotRepository _parkingSpotRepository;
    private readonly TripRepository _tripRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _unclaimedParkingSpotsHistogram;

    public ParkingSpotStepper(
        ILogger<ParkingSpotStepper> logger,
        ParkingSpotService parkingSpotService,
        ParkingSpotRepository parkingSpotRepository,
        TripRepository tripRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _parkingSpotService = parkingSpotService;
        _parkingSpotRepository = parkingSpotRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;

        _unclaimedParkingSpotsHistogram =
            meter.CreateHistogram<int>("unclaimed-parking-spot", "ParkingSpot", "#ParkingSpots Unclaimed.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for ParkingSpot in this Step \n({Step})",
            _modelState.ModelTime);

        var unclaimed = await _parkingSpotRepository.CountUnclaimedAsync(cancellationToken);
        _unclaimedParkingSpotsHistogram.Record(unclaimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for ParkingSpot in this Step \n({Step})",
            _modelState.ModelTime);
    }
    
    public async Task StepAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(parkingSpot, cancellationToken);

        if (trip != null)
        {
            _logger.LogDebug("ParkingSpot \n({@ParkingSpot})\n has an active Trip assigned in this Step \n({Step})",
                parkingSpot,
                _modelState.ModelTime);

            _logger.LogDebug("ParkingSpot \n({@ParkingSpot})\n will remain idle in this Step \n({Step})",
                parkingSpot,
                _modelState.ModelTime);

            return;
        }

        _logger.LogInformation("ParkingSpot \n({@ParkingSpot})\n has no active Trip assigned in this Step \n({Step})",
            parkingSpot,
            _modelState.ModelTime);
        
        _logger.LogDebug("Alerting Free for this ParkingSpot \n({@ParkingSpot})\n in this Step \n({Step})",
            parkingSpot,
            _modelState.ModelTime);
        await _parkingSpotService.AlertFreeAsync(parkingSpot, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var parkingSpots = (_parkingSpotRepository.Get())
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var parkingSpot in parkingSpots)
        {
            _logger.LogDebug("Handling Step \n({Step})\n for this ParkingSpot \n({@ParkingSpot})",
                _modelState.ModelTime,
                parkingSpot);
            
            await StepAsync(parkingSpot, cancellationToken);
            
            _logger.LogDebug("Completed handling Step \n({Step})\n for this ParkingSpot \n({@ParkingSpot})",
                _modelState.ModelTime,
                parkingSpot);
        }
    }
}