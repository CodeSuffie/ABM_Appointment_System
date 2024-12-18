using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Services.Abstractions;

public sealed class ModelStepper
{
    private readonly ILogger<ModelStepper> _logger;
    private readonly ModelState _modelState;
    private readonly IEnumerable<IStepperService> _stepperServices;
    private readonly Counter<int> _stepCounter;
    
    public ModelStepper(
        ILogger<ModelStepper> logger,
        ModelState modelState,
        IEnumerable<IStepperService> stepperServices,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _stepperServices = stepperServices;

        _stepCounter = meter.CreateCounter<int>("steps", "Steps", "Number of steps executed.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for this Step \n({Step})",
            _modelState.ModelTime);
        
        foreach (var stepperService in _stepperServices)
        {
            await stepperService.DataCollectAsync(cancellationToken);
        }
        
        _logger.LogDebug("Completed Data Collection for this Step \n({Step})",
            _modelState.ModelTime);
        
        _stepCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling this Step \n({Step})",
            _modelState.ModelTime);
        
        foreach (var stepperService in _stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
        
        _logger.LogDebug("Completed handling this Step \n({Step})",
            _modelState.ModelTime);
    }
}