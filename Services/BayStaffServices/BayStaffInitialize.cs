using Database;
using Services.Abstractions;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffInitialize(
    ModelDbContext context, 
    BayStaffService bayStaffService,
    BayShiftService bayShiftService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var bayStaff = await bayStaffService.GetNewObjectAsync(cancellationToken);
        
        await bayShiftService.GetNewObjectsAsync(bayStaff, cancellationToken);
        
        context.BayStaffs
            .Add(bayStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}