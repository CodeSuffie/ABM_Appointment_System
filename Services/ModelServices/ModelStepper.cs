using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper(
    ILogger<ModelStepper> logger,
    ModelState modelState,
    LoadService loadService,
    IEnumerable<IStepperService> stepperServices)
{
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await loadService.AddNewLoadsAsync(modelState.ModelConfig.LoadsPerStep, cancellationToken);
        
        logger.LogDebug("Handling this Step ({Step})...",
            modelState.ModelTime);
        
        foreach (var stepperService in stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
        
        logger.LogDebug("Completed handling this Step ({Step}).",
            modelState.ModelTime);

        await modelState.StepAsync(cancellationToken);
    }
}
