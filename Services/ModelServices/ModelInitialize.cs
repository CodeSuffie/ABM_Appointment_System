using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Settings;

namespace Services.ModelServices;

public sealed class ModelInitialize(
    ILogger<ModelInitialize> logger,
    ModelState modelState,
    IEnumerable<IPriorityInitializationService> initializationServices)
{
    public void InitializeObject()
    {
        modelState.ModelTime = new TimeSpan(0, 0, 0);
        modelState.ModelConfig = new ModelConfig();
        modelState.AgentConfig = new AgentConfig();
            
        logger.LogInformation("New ModelState created: ModelState={@ModelState}", modelState);
    }

    private async Task InitializeByPriorityAsync(Priority priority, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Initialization for Priority: {@Priority}...",
            priority);
        
        foreach (var initializationService in initializationServices
                     .Where(service => 
                         service.Priority == priority))
        {
            await initializationService.InitializeObjectsAsync(cancellationToken);
        }
        
        logger.LogInformation("Initialization Completed for Priority: {@Priority}...",
            priority);
    }
    
    public async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        InitializeObject();
        
        logger.LogInformation("Starting Initialization...");

        await InitializeByPriorityAsync(Priority.High, cancellationToken);
        await InitializeByPriorityAsync(Priority.Normal, cancellationToken);
        await InitializeByPriorityAsync(Priority.Low, cancellationToken);
        
        logger.LogInformation("Initialization Completed.");
    }
}