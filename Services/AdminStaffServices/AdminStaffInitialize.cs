using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffInitialize(
    ILogger<AdminStaffInitialize> logger,
    AdminStaffService adminStaffService,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Low;

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var adminStaff = await adminStaffService.GetNewObjectAsync(cancellationToken);
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