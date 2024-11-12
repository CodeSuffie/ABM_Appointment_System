using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper(IEnumerable<IStepperService> stepperServices)
{
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        // TODO: Update model time
        foreach (var stepperService in stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
    }
}
