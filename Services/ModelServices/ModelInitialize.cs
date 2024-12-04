using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.LoadServices;
using Settings;

namespace Services.ModelServices;

public sealed class ModelInitialize(
    ILogger<ModelInitialize> logger,
    ModelState modelState,
    LoadService loadService,
    IEnumerable<IPriorityInitializationService> priorityInitializationServices,
    IEnumerable<IInitializationService> initializationServices)
{
    public void InitializeObject()
    {
        modelState.ModelTime = new TimeSpan(0, 0, 0);
        modelState.ModelConfig = new ModelConfig();
        modelState.AgentConfig = new AgentConfig();
            
        logger.LogInformation("New ModelState created: ModelState={@ModelState}", modelState);
    }
    
    public async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        InitializeObject();
        
        logger.LogInformation("Starting Priority Initialization...");
        foreach (var priorityInitializationService in priorityInitializationServices)
        {
            await priorityInitializationService.InitializeObjectsAsync(cancellationToken);
        }
        logger.LogInformation("Priority Initialization Completed.");
        
        logger.LogInformation("Starting Initialization...");
        foreach (var initializationService in initializationServices)
        {
            await initializationService.InitializeObjectsAsync(cancellationToken);
        }
        logger.LogInformation("Initialization Completed.");
    }
}