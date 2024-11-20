using Database;
using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffInitialize(
    BayStaffService bayStaffService,
    BayShiftService bayShiftService,
    BayStaffRepository bayStaffRepository) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var bayStaff = await bayStaffService.GetNewObjectAsync(cancellationToken);
        
        await bayShiftService.GetNewObjectsAsync(bayStaff, cancellationToken);

        await bayStaffRepository.AddAsync(bayStaff, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}