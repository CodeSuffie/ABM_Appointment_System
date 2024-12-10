using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PickerServices;

public sealed class PickerInitialize(
    ILogger<PickerInitialize> logger,
    PickerService pickerService,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Low;
    
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var picker = await pickerService.GetNewObjectAsync(cancellationToken);
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