using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class PickerInitializer(
    ILogger<PickerInitializer> logger,
    PickerFactory pickerFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;
    
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var picker = await pickerFactory.GetNewObjectAsync(cancellationToken);
        if (picker == null)
        {
            logger.LogError("Could not construct a new Picker...");
            
            return;
        }
        
        logger.LogInformation("New Picker created: Picker={@Picker}", picker);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.PickerCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}