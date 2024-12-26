using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class LoadStepper : IStepperService
{
    private readonly ILogger<LoadStepper> _logger;
    private readonly LoadRepository _loadRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _unclaimedLoadsHistogram;
    
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
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Load in this Step ({Step})", _modelState.ModelTime);
        
        // var unclaimed = await _loadRepository.CountUnclaimedAsync(cancellationToken);
        // _unclaimedLoadsHistogram.Record(unclaimed, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Load in this Step ({Step})", _modelState.ModelTime);
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}