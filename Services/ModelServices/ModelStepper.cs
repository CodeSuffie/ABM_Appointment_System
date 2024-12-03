using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper
{
    private readonly ILogger<ModelStepper> _logger;
    private readonly ModelState _modelState;
    private readonly LoadService _loadService;
    private readonly IEnumerable<IStepperService> _stepperServices;
    private readonly Counter<int> _stepCounter;
    
    public ModelStepper(
        ILogger<ModelStepper> logger,
        ModelState modelState,
        LoadService loadService,
        IEnumerable<IStepperService> stepperServices,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _loadService = loadService;
        _stepperServices = stepperServices;

        _stepCounter = meter.CreateCounter<int>("steps", "Steps", "Number of steps executed.");
    }
    
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        if (_modelState.ModelTime > _modelState.ModelConfig.ModelTime)
        {
            return;
        }
        
        await _loadService.AddNewLoadsAsync(_modelState.ModelConfig.LoadsPerStep, cancellationToken);
        
        _logger.LogDebug("Handling this Step \n({Step})",
            _modelState.ModelTime);
        
        foreach (var stepperService in _stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
        
        _stepCounter.Add(1);
        
        _logger.LogDebug("Completed handling this Step \n({Step})",
            _modelState.ModelTime);

        await _modelState.StepAsync(cancellationToken);
    }
}
