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
    private readonly Histogram<int> _closedBaysHistogram;
    private readonly Histogram<int> _freeBaysHistogram;
    private readonly Histogram<int> _claimedBaysHistogram;
    private readonly Histogram<int> _droppedOffBaysHistogram;
    private readonly Histogram<int> _fetchedBaysHistogram;
    private readonly Histogram<int> _pickedUpBaysHistogram;
    
    public BayStepper(
        ILogger<BayStepper> logger,
        BayService bayService,
        BayRepository bayRepository,
        TripRepository tripRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _bayService = bayService;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _modelState = modelState;

        _closedBaysHistogram = meter.CreateHistogram<int>("closed-bay", "Bay", "#Bays Closed.");
        _freeBaysHistogram = meter.CreateHistogram<int>("free-bay", "Bay", "#Bays Free.");
        _claimedBaysHistogram = meter.CreateHistogram<int>("claimed-bay", "Bay", "#Bays Claimed.");
        _droppedOffBaysHistogram = meter.CreateHistogram<int>("dropped-off-bay", "Bay", "#Bays Finished Drop-Off.");
        _fetchedBaysHistogram = meter.CreateHistogram<int>("fetched-bay", "Bay", "#Bays Finished Fetching.");
        _pickedUpBaysHistogram = meter.CreateHistogram<int>("picking-up-bay", "Bay", "#Bays Working on a Pick-Up.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Bay in this Step ({Step})", _modelState.ModelTime);
        
        // var closed = await _bayRepository.CountAsync(BayStatus.Closed, cancellationToken);
        // _closedBaysHistogram.Record(closed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var free = await _bayRepository.CountAsync(false, cancellationToken);
        // _freeBaysHistogram.Record(free, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var claimed = await _bayRepository.CountAsync(true, cancellationToken);
        // _claimedBaysHistogram.Record(claimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var droppedOff = await _bayRepository.CountAsync(BayFlags.DroppedOff, cancellationToken);
        // _droppedOffBaysHistogram.Record(droppedOff, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var fetched = await _bayRepository.CountAsync(BayFlags.Fetched, cancellationToken);
        // _fetchedBaysHistogram.Record(fetched, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var pickedUp = await _bayRepository.CountAsync(BayFlags.PickedUp, cancellationToken);
        // _pickedUpBaysHistogram.Record(pickedUp, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Bay in this Step ({Step})", _modelState.ModelTime);
    }
    
    public async Task StepAsync(Bay bay, CancellationToken cancellationToken)
    {
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