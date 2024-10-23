using System.Runtime.Loader;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class AdminStaffService(ModelDbContext context) : IAgentService<AdminStaff>
{
    private readonly ModelDbContext _context = context;

    public async Task InitializeAgentShiftAsync(AdminStaff adminStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.AdminStaffAverageWorkDays) return;
        // TODO: So the Work Day amount is depended on the Operating Day amount for their Hub???
            
        var maxShiftStart = operatingHour.Duration.Value - 
                            AgentConfig.AdminShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return;
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var shift = new AdminShift {
            AdminStaff = adminStaff,
            StartTime = operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.AdminShiftAverageLength
        };
            
        adminStaff.Shifts.Add(shift);
    }

    public async Task InitializeAgentShiftsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours.Where(
            x => x.HubId == adminStaff.Hub.Id
        );
        
        foreach (var operatingHour in operatingHours)
        {
            await InitializeAgentShiftAsync(adminStaff, operatingHour, cancellationToken);
        }
    }

    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var adminStaff = new AdminStaff
        {
            Hub = hub
        };
        
        await InitializeAgentShiftsAsync(adminStaff, cancellationToken);
        
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
        var adminStaffs = await _context.AdminStaffs.ToListAsync(cancellationToken);
        foreach (var adminStaff in adminStaffs)
        {
            await ExecuteStepAsync(adminStaff, cancellationToken);
        }
    }
}
