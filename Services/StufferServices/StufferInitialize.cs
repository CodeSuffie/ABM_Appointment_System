using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.StufferServices;

public sealed class StufferInitialize(
    ILogger<StufferInitialize> logger,
    StufferService stufferService,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Low;
    
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var stuffer = await stufferService.GetNewObjectAsync(cancellationToken);
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