using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.LoadServices;

public sealed class LoadStepper : IStepperService
{
    private readonly ILogger<LoadStepper> _logger;
    private readonly LoadRepository _loadRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _unclaimedLoadsHistogram;
    private readonly Histogram<int> _droppedOffLoadsHistogram;
    
    public LoadStepper(
        ILogger<LoadStepper> logger,
        LoadRepository loadRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _loadRepository = loadRepository;
        _modelState = modelState;

        _unclaimedLoadsHistogram = meter.CreateHistogram<int>("unclaimed-load", "Load", "#Loads Unclaimed.");
        _droppedOffLoadsHistogram = meter.CreateHistogram<int>("dropped-off-load", "Load", "#Loads Dropped Off.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Load in this Step \n({Step})",
            _modelState.ModelTime);
        
        var unclaimed = await _loadRepository.CountUnclaimedAsync(cancellationToken);
        _unclaimedLoadsHistogram.Record(unclaimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var droppedOff = await _loadRepository.CountDroppedOffAsync(cancellationToken);
        _droppedOffLoadsHistogram.Record(droppedOff, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Load in this Step \n({Step})",
            _modelState.ModelTime);
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}