using Database;
using Database.Models;
using Services.HubServices;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffService(
    ModelDbContext context,
    HubService hubService) 
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
    
    public double GetWorkChance(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = context.Hubs.FirstOrDefault(x => x.Id == bayStaff.HubId);
        if (hub == null) throw new Exception("This BayStaff did not have a Hub assigned.");
        
        return bayStaff.WorkChance / hub.OperatingChance;
    }
    
    
}
