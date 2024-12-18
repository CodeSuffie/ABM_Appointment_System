using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class BayStaffInitializer(
    ILogger<BayStaffInitializer> logger,
    BayStaffFactory bayStaffFactory,
    BayShiftFactory bayShiftFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;
    
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var bayStaff = await bayStaffFactory.GetNewObjectAsync(cancellationToken);
        if (bayStaff == null)
        {
            logger.LogError("Could not construct a new BayStaff...");
            
            return;
        }
        
        logger.LogInformation("New BayStaff created: BayStaff={@BayStaff}", bayStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }

        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            await bayShiftFactory.GetNewObjectsAsync(cancellationToken);
        }
    }
}