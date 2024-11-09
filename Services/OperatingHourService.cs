using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class OperatingHourService(ModelDbContext context)
{
    private async Task<TimeSpan> GetStartTimeAsync(
        Hub hub, 
        TimeSpan day, 
        CancellationToken cancellationToken)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            hub.AverageOperatingHourLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This Hub its OperatingHourLength is longer than a full day.");      
        // Hub Operating Hours can be longer than 1 day?
            
        var operatingHourHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        return day + new TimeSpan(operatingHourHour, operatingHourMinutes, 0);
    }
    
    private async Task<OperatingHour> GetNewObjectsAsync(Hub hub, TimeSpan day, CancellationToken cancellationToken)
    {
        var startTime = await GetStartTimeAsync(hub, day, cancellationToken);
        
        var operatingHour = new OperatingHour {
            Hub = hub,
            StartTime = startTime,
            Duration = hub.AverageOperatingHourLength,
        };

        return operatingHour;
    }

    public async Task GetNewObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ModelConfig.ModelTime.Days; i++)
        {
            if (ModelConfig.Random.NextDouble() > hub.OperatingChance) continue;
            
            var operatingHour = await GetNewObjectsAsync(hub, TimeSpan.FromDays(i), cancellationToken);
            
            hub.OperatingHours.Add(operatingHour);
        }
    }
}