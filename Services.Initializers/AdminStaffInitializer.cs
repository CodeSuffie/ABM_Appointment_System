using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class AdminStaffInitializer(
    ILogger<AdminStaffInitializer> logger,
    AdminStaffFactory adminStaffFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var adminStaff = await adminStaffFactory.GetNewObjectAsync(cancellationToken);
        if (adminStaff == null)
        {
            logger.LogError("Could not construct a new AdminStaff...");
            
            return;
        }
        
        logger.LogInformation("New AdminStaff created: AdminStaff={@AdminStaff}", adminStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.AdminStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}