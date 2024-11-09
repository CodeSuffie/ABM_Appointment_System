using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper(IEnumerable<IStepperService> stepperServices)
{
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        foreach (var stepperService in stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
    }
}
