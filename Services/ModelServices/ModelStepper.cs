using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelStepper(IEnumerable<IStepperService> stepperServices)
{
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        // TODO: Update model time
        // TODO: Optionally add new Loads [loadService.AddNewLoads(count, cancellationToken)]
        foreach (var stepperService in stepperServices)
        {
            await stepperService.StepAsync(cancellationToken);
        }
    }
}
