using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
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
    
    // TODO: Repository
    public async Task<Hub> GetHubForAdminStaffAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == adminStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This AdminStaff did not have a Hub assigned.");

        return hub;
    }
    
    // TODO: Repository
    public async Task<Work?> GetWorkForAdminStaffAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.AdminStaff != null &&
                                      x.AdminStaffId == adminStaff.Id, cancellationToken);
        
        return work;
    }

    // TODO: Repository
    public async Task<IQueryable<AdminShift>> GetShiftsForAdminStaffAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = context.AdminShifts
            .Where(x => x.AdminStaffId == adminStaff.Id);

        return shifts;
    }
    
    public async Task<double> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await GetHubForAdminStaffAsync(adminStaff, cancellationToken);
        
        return adminStaff.WorkChance / hub.OperatingChance;
    }
}
