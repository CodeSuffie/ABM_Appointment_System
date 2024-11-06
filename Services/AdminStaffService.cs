using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class AdminStaffService(
    ModelDbContext context, 
    AdminShiftService adminShiftService
    ) : IInitializationService, IStepperService<AdminStaff>
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = await context.Hubs
            .ToListAsync(cancellationToken);
        
        if (hubs.Count <= 0)
            throw new Exception("There was no Hub to assign this new AdminStaff to.");
        
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var adminStaff = new AdminStaff
        {
            Hub = hub
        };
        
        await adminShiftService.InitializeObjectsAsync(adminStaff, cancellationToken);
        
        context.AdminStaffs
            .Add(adminStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.AdminStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = await context.AdminStaffs
            .ToListAsync(cancellationToken);
        
        foreach (var adminStaff in adminStaffs)
        {
            await ExecuteStepAsync(adminStaff, cancellationToken);
        }
    }
}
