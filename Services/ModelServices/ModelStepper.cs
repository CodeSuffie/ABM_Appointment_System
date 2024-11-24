using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper(
    ModelState modelState,
    LoadService loadService,
    IEnumerable<IStepperService> stepperServices)
{
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await loadService.AddNewLoadsAsync(modelState.ModelConfig.LoadsPerStep, cancellationToken);
        
        foreach (var stepperService in stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
    }
}
