using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayStaffService(ModelDbContext context) : IAgentService<BayStaff>
{
    private async Task InitializeAgentShiftAsync(BayStaff bayStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.BayStaffAverageWorkDays) return;
        // TODO: So the Work Day amount is depended on the Operating Day amount for their Hub???
            
        var maxShiftStart = operatingHour.Duration.Value - 
                            AgentConfig.BayShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return;
        
        var bays = context.Bays.Where(
            x => x.HubId == bayStaff.Hub.Id
        ).ToList();
        
        var bay = bays[ModelConfig.Random.Next(bays.Count)];
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var shift = new BayShift {
            BayStaff = bayStaff,
            Bay = bay,
            StartTime = operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.BayShiftAverageLength,
        };
            
        bayStaff.Shifts.Add(shift);
    }

    private async Task InitializeAgentShiftsAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours.Where(
            x => x.HubId == bayStaff.Hub.Id
        );
        
        foreach (var operatingHour in operatingHours)
        {
            await InitializeAgentShiftAsync(bayStaff, operatingHour, cancellationToken);
        }
    }

    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var bayStaff = new BayStaff
        {
            Hub = hub
        };
        
        await InitializeAgentShiftsAsync(bayStaff, cancellationToken);
        
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
