using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.ModelServices;

public sealed class ModelInitialize(
    ModelState modelState,
    LoadService loadService,
    IEnumerable<IPriorityInitializationService> priorityInitializationServices,
    IEnumerable<IInitializationService> initializationServices) : IInitializationService
{
    public Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        modelState.ModelTime = new TimeSpan(0, 0, 0);
        modelState.ModelConfig = new ModelConfig();
        modelState.AgentConfig = new AgentConfig();
        return Task.CompletedTask;
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await InitializeObjectAsync(cancellationToken);
    }
    
    public async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        foreach (var priorityInitializationService in priorityInitializationServices)
        {
            await priorityInitializationService.InitializeObjectsAsync(cancellationToken);
        }
        
        foreach (var initializationService in initializationServices)
        {
            await initializationService.InitializeObjectsAsync(cancellationToken);
        }
        
        await loadService.AddNewLoadsAsync(modelState.ModelConfig.InitialLoads, cancellationToken);
    }
}