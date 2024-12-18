using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class StufferInitializer(
    ILogger<StufferInitializer> logger,
    StufferFactory stufferFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;
    
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var stuffer = await stufferFactory.GetNewObjectAsync(cancellationToken);
        if (stuffer == null)
        {
            logger.LogError("Could not construct a new Stuffer...");
            
            return;
        }
        
        logger.LogInformation("New Picker created: Stuffer={@Stuffer}", stuffer);
    }
    
    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.StufferCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}