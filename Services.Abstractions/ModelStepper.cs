using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Settings;

namespace Services.Abstractions;

public sealed class ModelStepper
{
    private readonly ILogger<ModelStepper> _logger;
    private readonly ModelState _modelState;
    private readonly IEnumerable<IStepperService> _stepperServices;
    private readonly Instrumentation _instrumentation;
    
    public ModelStepper(
        ILogger<ModelStepper> logger,
        ModelState modelState,
        IEnumerable<IStepperService> stepperServices,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _modelState = modelState;
        _stepperServices = stepperServices;
        _instrumentation = instrumentation; 
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Handling this Step ({Step})", _modelState.ModelTime);
        
        foreach (var stepperService in _stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
        
        _logger.LogInformation("Completed handling this Step ({Step})", _modelState.ModelTime);
    }
}
