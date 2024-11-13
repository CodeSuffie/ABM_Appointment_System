using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
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
    
    public async Task<Hub> GetHubForBayStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await context.Hubs
            .FirstOrDefaultAsync(x => x.Id == bayStaff.HubId, cancellationToken);
        if (hub == null) throw new Exception("This BayStaff did not have a Hub assigned.");

        return hub;
    }
    
    public async Task<Work?> GetWorkForBayStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.BayStaff != null &&
                                      x.BayStaffId == bayStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<IQueryable<BayShift>> GetShiftsForBayStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayStaffId == bayStaff.Id);

        return shifts;
    }
    
    public async Task<double> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await GetHubForBayStaffAsync(bayStaff, cancellationToken);
        
        return bayStaff.WorkChance / hub.OperatingChance;
    }
    
    
}
