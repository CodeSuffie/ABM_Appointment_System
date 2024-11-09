using Database;
using Database.Models;
using Services.HubServices;
using Settings;

namespace Services.AdminStaffServices;

public sealed class AdminStaffService(
    ModelDbContext context,
    HubService hubService) 
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
    
    public double GetWorkChance(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = context.Hubs.FirstOrDefault(x => x.Id == adminStaff.HubId);
        if (hub == null) throw new Exception("This AdminStaff did not have a Hub assigned.");
        
        return adminStaff.WorkChance / hub.OperatingChance;
    }
}
