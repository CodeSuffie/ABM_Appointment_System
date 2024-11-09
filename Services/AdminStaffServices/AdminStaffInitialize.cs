using Database;
using Services.Abstractions;
using Settings;

namespace Services.AdminStaffServices;

public sealed class AdminStaffInitialize(
    ModelDbContext context, 
    AdminStaffService adminStaffService,
    AdminShiftService adminShiftService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var adminStaff = await adminStaffService.GetNewObjectAsync(cancellationToken);
        
        await adminShiftService.GetNewObjectsAsync(adminStaff, cancellationToken);
        
        context.AdminStaffs
            .Add(adminStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.AdminStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}