namespace Services.Abstractions;

public interface IAgentService
{
    Task InitializeAgentsAsync(CancellationToken cancellationToken);
    Task ExecuteStepAsync(CancellationToken cancellationToken);
}

public interface IAgentService<in TModel> : IAgentService where TModel : class
{
    Task ExecuteStepAsync(TModel entity, CancellationToken cancellationToken);
}
