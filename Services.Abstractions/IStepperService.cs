namespace Services.Abstractions;

public interface IStepperService
{
    Task ExecuteStepAsync(CancellationToken cancellationToken);
}

public interface IStepperService<in TModel> : IStepperService where TModel : class
{
    Task ExecuteStepAsync(TModel entity, CancellationToken cancellationToken);
}