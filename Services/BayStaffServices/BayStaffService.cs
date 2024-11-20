using Database.Models;
using Repositories;
using Services.HubServices;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffService(
    HubService hubService,
    HubRepository hubRepository) 
{
    public async Task<BayStaff> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        
        var bayStaff = new BayStaff
        {
            Hub = hub,
            WorkChance = AgentConfig.BayStaffAverageWorkDays,
            AverageShiftLength = AgentConfig.BayShiftAverageLength
        };

        return bayStaff;
    }
    
    public async Task<double> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetHubByStaffAsync(bayStaff, cancellationToken);
        
        return bayStaff.WorkChance / hub.OperatingChance;
    }
}
