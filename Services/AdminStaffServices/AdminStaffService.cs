using Database.Models;
using Repositories;
using Services.HubServices;
using Settings;

namespace Services.AdminStaffServices;

public sealed class AdminStaffService(
    HubService hubService,
    HubRepository hubRepository)
{
    public async Task<AdminStaff> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        
        var adminStaff = new AdminStaff
        {
            Hub = hub,
            WorkChance = AgentConfig.AdminStaffAverageWorkDays,
            AverageShiftLength = AgentConfig.AdminShiftAverageLength
        };

        return adminStaff;
    }
    
    public async Task<double> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetHubByStaffAsync(adminStaff, cancellationToken);
        
        return adminStaff.WorkChance / hub.OperatingChance;
    }
}
