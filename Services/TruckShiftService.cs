using Database;
using Database.Models;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckShiftService(ModelDbContext context)
{
    private static TruckShift? GetNewObject(TruckDriver truckDriver, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            AgentConfig.TruckShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return null;      // Truck Drivers are working longer than 1 day shifts?
        
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours
            ? ModelConfig.Random.Next(maxShiftStart.Minutes)
            : ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var truckShift = new TruckShift {
            TruckDriver = truckDriver,
            StartTime = startTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.TruckShiftAverageLength,
        };

        return truckShift;
    }
    
    public static async Task InitializeObjectAsync(TruckDriver truckDriver, TimeSpan startTime, CancellationToken cancellationToken)
    {
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.TruckDriverAverageWorkDays) return;
        
        var truckShift = GetNewObject(truckDriver, startTime, cancellationToken);
        if (truckShift != null)
        {
            truckDriver.Shifts.Add(truckShift);
        }
    }

    public async Task InitializeObjectsAsync(TruckDriver truckDriver, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ModelConfig.ModelTime.Days; i++)
        {
            await InitializeObjectAsync(truckDriver, TimeSpan.FromDays(i), cancellationToken);
        }
    }
}