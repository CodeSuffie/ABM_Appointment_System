using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.BayServices;

public sealed class BayStepper : IStepperService
{
    private readonly ILogger<BayStepper> _logger;
    private readonly BayRepository _bayRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _closedHubsHistogram;
    private readonly Histogram<int> _freeHubsHistogram;
    private readonly Histogram<int> _claimedHubsHistogram;
    private readonly Histogram<int> _droppingOffHubsHistogram;
    private readonly Histogram<int> _droppedOffHubsHistogram;
    private readonly Histogram<int> _fetchingHubsHistogram;
    private readonly Histogram<int> _fetchedHubsHistogram;
    private readonly Histogram<int> _pickingUpHubsHistogram;
    
    public BayStepper(
        ILogger<BayStepper> logger,
        BayRepository bayRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _bayRepository = bayRepository;
        _modelState = modelState;

        _closedHubsHistogram = meter.CreateHistogram<int>("closed-bay", "Bay", "#Bays Closed.");
        _freeHubsHistogram = meter.CreateHistogram<int>("free-bay", "Bay", "#Bays Free.");
        _claimedHubsHistogram = meter.CreateHistogram<int>("claimed-bay", "Bay", "#Bays Claimed.");
        _droppingOffHubsHistogram = meter.CreateHistogram<int>("dropping-off-bay", "Bay", "#Bays Working on a Drop-Off.");
        _droppedOffHubsHistogram = meter.CreateHistogram<int>("dropped-off-bay", "Bay", "#Bays Finished Drop-Off.");
        _fetchingHubsHistogram = meter.CreateHistogram<int>("fetching-bay", "Bay", "#Bays Working on a Fetch.");
        _fetchedHubsHistogram = meter.CreateHistogram<int>("fetched-bay", "Bay", "#Bays Finished Fetching.");
        _pickingUpHubsHistogram = meter.CreateHistogram<int>("picking-up-bay", "Bay", "#Bays Working on a Pick-Up.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Bay in this Step \n({Step})",
            _modelState.ModelTime);
        
        var closed = await _bayRepository.CountAsync(BayStatus.Closed, cancellationToken);
        _closedHubsHistogram.Record(closed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var free = await _bayRepository.CountAsync(BayStatus.Free, cancellationToken);
        _freeHubsHistogram.Record(free, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var claimed = await _bayRepository.CountAsync(BayStatus.Claimed, cancellationToken);
        _claimedHubsHistogram.Record(claimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var fetching = await _bayRepository.CountAsync(BayStatus.FetchStarted, cancellationToken);
        _fetchingHubsHistogram.Record(fetching, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var fetched = await _bayRepository.CountAsync(BayStatus.FetchFinished, cancellationToken);
        _fetchedHubsHistogram.Record(fetched, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var droppedOff = await _bayRepository.CountAsync(BayStatus.WaitingFetchStart, cancellationToken);
        _droppedOffHubsHistogram.Record(droppedOff, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var droppingOffStarted = await _bayRepository.CountAsync(BayStatus.DroppingOffStarted, cancellationToken);
        var droppingOff = droppingOffStarted + fetching + fetched;
        _droppingOffHubsHistogram.Record(droppingOff, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var pickingUp = await _bayRepository.CountAsync(BayStatus.PickUpStarted, cancellationToken);
        _pickingUpHubsHistogram.Record(pickingUp, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Bay in this Step \n({Step})",
            _modelState.ModelTime);
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}