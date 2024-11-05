using Database.Models;
using Settings;

namespace Services;

public static class BayShiftService
{
    private static double GetBayStaffWorkChance(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        return AgentConfig.BayStaffAverageWorkDays / AgentConfig.HubAverageOperatingDays;
    }
    
    private static BayShift? GetNewObject(BayStaff bayStaff, OperatingHour operatingHour, List<Bay> bays, CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            AgentConfig.BayShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return null;

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
            
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
    
    public static async Task InitializeObjectAsync(BayStaff bayStaff, OperatingHour operatingHour, List<Bay> bays, CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            GetBayStaffWorkChance(bayStaff, cancellationToken)) return;
            
        var bayShift = GetNewObject(bayStaff, operatingHour, bays, cancellationToken);
        if (bayShift != null)
        {
            bayStaff.Shifts.Add(bayShift);
        }
    }

    public static async Task InitializeObjectsAsync(BayStaff bayStaff, List<OperatingHour> operatingHours, List<Bay> bays, CancellationToken cancellationToken)
    {
        foreach (var operatingHour in operatingHours)
        {
            await InitializeObjectAsync(bayStaff, operatingHour, bays, cancellationToken);
        }
    }
}