using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Services;

public sealed class BayShiftService(ModelDbContext context)
{
    private double GetBayStaffWorkChance(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        return AgentConfig.BayStaffAverageWorkDays / AgentConfig.HubAverageOperatingDays;
    }
    
    private async Task<BayShift> GetNewObject(
        BayStaff bayStaff, 
        OperatingHour operatingHour, 
        CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            AgentConfig.BayShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This BayStaff its BayShiftLength is longer than the Hub its OperatingHourLength.");
        
        // ---
        var bays = await context.Bays
            .Where(x => x.HubId == bayStaff.Hub.Id)
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to the Hub of this BayStaff.");

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
        // -- TODO: Move to separate method
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);
        
        var bayShift = new BayShift {
            BayStaff = bayStaff,
            Bay = bay,
            StartTime = operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.BayShiftAverageLength,
        };

        return bayShift;
    }
    
    public async Task InitializeObjectAsync(
        BayStaff bayStaff, 
        OperatingHour operatingHour, 
        CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            GetBayStaffWorkChance(bayStaff, cancellationToken)) return;
            
        var bayShift = await GetNewObject(bayStaff, operatingHour, cancellationToken);
        bayStaff.Shifts.Add(bayShift);
    }

    public async Task InitializeObjectsAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours
            .Where(x => x.HubId == bayStaff.Hub.Id)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            await InitializeObjectAsync(bayStaff, operatingHour, cancellationToken);
        }
    }
}