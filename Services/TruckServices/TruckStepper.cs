using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TruckServices;

public sealed class TruckStepper : IStepperService<Truck>
{
    private readonly ILogger<TruckStepper> _logger;
    private readonly TruckService _truckService;
    private readonly TruckRepository _truckRepository;
    private readonly TripRepository _tripRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _unclaimedTrucksHistogram;

    public TruckStepper(
        ILogger<TruckStepper> logger,
        TruckService truckService,
        TruckRepository truckRepository,
        TripRepository tripRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _truckService = truckService;
        _truckRepository = truckRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;

        _unclaimedTrucksHistogram = meter.CreateHistogram<int>("unclaimed-truck", "Truck", "#Trucks Unclaimed.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Truck in this Step \n({Step})",
            _modelState.ModelTime);
        
        var unclaimed = await _truckRepository.CountUnclaimedAsync(cancellationToken);
        _unclaimedTrucksHistogram.Record(unclaimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Truck in this Step \n({Step})",
            _modelState.ModelTime);
    }
    
    public async Task StepAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(truck, cancellationToken);
        if (trip != null)
        {
            _logger.LogDebug("Truck \n({@Truck})\n has an active Trip assigned in this Step \n({Step})",
                truck,
                _modelState.ModelTime);
            
            _logger.LogDebug("Truck \n({@Truck})\n will remain idle in this Step \n({Step})",
                truck,
                _modelState.ModelTime);
            
            return;
        }
        
        _logger.LogInformation("Truck \n({@Truck})\n has no active Trip assigned in this Step \n({Step})",
            truck,
            _modelState.ModelTime);

        _logger.LogDebug("Alerting Free for this Truck \n({@Truck})\n in this Step \n({Step})",
            truck,
            _modelState.ModelTime);
        await _truckService.AlertFreeAsync(truck, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trucks = _truckRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            _logger.LogDebug("Handling Step \n({Step})\n for Truck \n({@Truck})",
                _modelState.ModelTime,
                truck);
            
            await StepAsync(truck, cancellationToken);
            
            _logger.LogDebug("Completed handling Step \n({Step})\n for Truck \n({@Truck})",
                _modelState.ModelTime,
                truck);
        }
    }
}