using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class AdminStaffService(ModelDbContext context) : IAgentService<AdminStaff>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var adminStaff = new AdminStaff
        {
            Hub = hub
        };
        
        var operatingHours = context.OperatingHours.Where(
            x => x.HubId == adminStaff.Hub.Id
        ).ToList();
        
        await AdminShiftService.InitializeObjectsAsync(adminStaff, operatingHours, cancellationToken);
        
        context.AdminStaffs.Add(adminStaff);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.AdminStaffCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = await context.AdminStaffs.ToListAsync(cancellationToken);
        foreach (var adminStaff in adminStaffs)
        {
            await ExecuteStepAsync(adminStaff, cancellationToken);
        }
    }
}
