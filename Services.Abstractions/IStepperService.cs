namespace Services.Abstractions;

public interface IStepperService
{
    Task DataCollectAsync(CancellationToken cancellationToken);
    Task StepAsync(CancellationToken cancellationToken);
}

public interface IStepperService<in TModel> : IStepperService where TModel : class
{
    Task StepAsync(TModel entity, CancellationToken cancellationToken);
}