using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayStaffService(
    ModelDbContext context, 
    BayShiftService bayShiftService
    ) : IAgentService<BayStaff>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var bayStaff = new BayStaff
        {
            Hub = hub
        };
        
        await bayShiftService.InitializeObjectsAsync(bayStaff, cancellationToken);
        
        context.BayStaffs.Add(bayStaff);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayStaffCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = await context.BayStaffs.ToListAsync(cancellationToken);
        foreach (var bayStaff in bayStaffs)
        {
            await ExecuteStepAsync(bayStaff, cancellationToken);
        }
    }
}
