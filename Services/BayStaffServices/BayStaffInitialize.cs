using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffInitialize(
    BayStaffService bayStaffService,
    BayShiftService bayShiftService,
    BayStaffRepository bayStaffRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var bayStaff = await bayStaffService.GetNewObjectAsync(cancellationToken);
        
        await bayShiftService.GetNewObjectsAsync(bayStaff, cancellationToken);

        await bayStaffRepository.AddAsync(bayStaff, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}