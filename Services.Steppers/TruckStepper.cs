using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class TruckStepper : IStepperService<Truck>
{
    private readonly ILogger<TruckStepper> _logger;
    private readonly TruckService _truckService;
    private readonly TruckRepository _truckRepository;
    private readonly TripRepository _tripRepository;
    private readonly ModelState _modelState;

    public TruckStepper(
        ILogger<TruckStepper> logger,
        TruckService truckService,
        TruckRepository truckRepository,
        TripRepository tripRepository,
        ModelState modelState)
    {
        _logger = logger;
        _truckService = truckService;
        _truckRepository = truckRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;
    }
    
    public async Task StepAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(truck, cancellationToken);
        if (trip != null)
        {
            _logger.LogDebug("Truck \n({@Truck})\n has an active Trip assigned in this Step ({Step})", truck, _modelState.ModelTime);
            
            _logger.LogDebug("Truck \n({@Truck})\n will remain idle in this Step ({Step})", truck, _modelState.ModelTime);
            
            return;
        }
        
        _logger.LogInformation("Truck \n({@Truck})\n has no active Trip assigned in this Step ({Step})", truck, _modelState.ModelTime);

        _logger.LogDebug("Alerting Free for this Truck \n({@Truck})\n in this Step ({Step})", truck, _modelState.ModelTime);
        await _truckService.AlertFreeAsync(truck, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trucks = _truckRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            _logger.LogDebug("Handling Step ({Step})\n for Truck \n({@Truck})", _modelState.ModelTime, truck);
            
            await StepAsync(truck, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for Truck \n({@Truck})", _modelState.ModelTime, truck);
        }
    }
}