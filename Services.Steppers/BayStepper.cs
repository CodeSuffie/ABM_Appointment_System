using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class BayStepper : IStepperService<Bay>
{
    private readonly ILogger<BayStepper> _logger;
    private readonly BayService _bayService;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly ModelState _modelState;
    
    public BayStepper(
        ILogger<BayStepper> logger,
        BayService bayService,
        BayRepository bayRepository,
        TripRepository tripRepository,
        ModelState modelState)
    {
        _logger = logger;
        _bayService = bayService;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;
    }
    
    public async Task StepAsync(Bay bay, CancellationToken cancellationToken)
    {
        await _bayService.UpdateStatusAsync(bay, cancellationToken);
        
        if (bay.BayStatus == BayStatus.Closed) return;
        
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            await _bayService.AlertFreeAsync(bay, cancellationToken);
            return;
        }
        
        await _bayService.UpdateFlagsAsync(bay, cancellationToken);
        
        if (bay.BayFlags.HasFlag(BayFlags.DroppedOff) && 
            bay.BayFlags.HasFlag(BayFlags.Fetched) &&
            bay.BayFlags.HasFlag(BayFlags.PickedUp))
        {
            await _bayService.AlertWorkCompleteAsync(bay, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bays = _bayRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bay in bays)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this Bay \n({@Bay})", _modelState.ModelTime, bay);
            
            await StepAsync(bay, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this Bay \n({@Bay})", _modelState.ModelTime, bay);
        }
    }
}