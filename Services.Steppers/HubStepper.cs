using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class HubStepper : IStepperService
{
    private readonly ILogger<HubStepper> _logger;
    private readonly ModelState _modelState;
    
    public HubStepper(
        ILogger<HubStepper> logger,
        ModelState modelState)
    {
        _logger = logger;
        _modelState = modelState;
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Hub in this Step ({Step})", _modelState.ModelTime);
        
        // var operating = await _hubRepository.CountOperatingAsync(_modelState.ModelTime, cancellationToken);
        // _operatingHubsHistogram.Record(operating, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Hub in this Step ({Step})", _modelState.ModelTime);
    }
    
    public Task StepAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}