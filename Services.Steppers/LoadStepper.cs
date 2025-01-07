using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class LoadStepper : IStepperService
{
    private readonly ILogger<LoadStepper> _logger;
    private readonly ModelState _modelState;
    
    public LoadStepper(
        ILogger<LoadStepper> logger,
        ModelState modelState)
    {
        _logger = logger;
        _modelState = modelState;
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