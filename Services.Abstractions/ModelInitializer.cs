using Microsoft.Extensions.Logging;
using Settings;

namespace Services.Abstractions;

public sealed class ModelInitializer(
    ILogger<ModelInitializer> logger,
    ModelState modelState,
    IEnumerable<IPriorityInitializerService> initializationServices)
{
    public void InitializeObject()  
    {
        modelState.Initialize();
        
        // modelState.Initialize(
        //     new TimeSpan(0, 0, 0),
        //     new AppointmentModelConfig(),
        //     new AppointmentAgentConfig(),
        //     new AppointmentConfig());
            
        logger.LogInformation("New ModelState created: ModelState={@ModelState}", modelState);
    }

    private async Task InitializeByPriorityAsync(Priority priority, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Initialization for Priority: {@Priority}...", priority);
        
        foreach (var initializationService in initializationServices
                     .Where(service => 
                         service.Priority == priority))
        {
            await initializationService.InitializeObjectsAsync(cancellationToken);
        }
        
        logger.LogInformation("Initialization Completed for Priority: {@Priority}...", priority);
    }
    
    public async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        InitializeObject();
        
        logger.LogInformation("Starting Initialization...");

        await InitializeByPriorityAsync(Priority.High, cancellationToken);
        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            await InitializeByPriorityAsync(Priority.Appointment, cancellationToken);
        }
        await InitializeByPriorityAsync(Priority.Normal, cancellationToken);
        await InitializeByPriorityAsync(Priority.Low, cancellationToken);
        
        logger.LogInformation("Initialization Completed.");
    }
}