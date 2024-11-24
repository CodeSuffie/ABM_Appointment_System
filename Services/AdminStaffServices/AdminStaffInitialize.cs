using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.AdminStaffServices;

public sealed class AdminStaffInitialize(
    AdminStaffService adminStaffService,
    AdminShiftService adminShiftService,
    AdminStaffRepository adminStaffRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var adminStaff = await adminStaffService.GetNewObjectAsync(cancellationToken);
        
        await adminShiftService.GetNewObjectsAsync(adminStaff, cancellationToken);

        await adminStaffRepository.AddAsync(adminStaff, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.AdminStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}