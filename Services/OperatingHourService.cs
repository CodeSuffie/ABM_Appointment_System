using Database.Models;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class OperatingHourService(
    ModelState modelState)
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
            
        var operatingHourHour = modelState.Random(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

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
        for (var i = 0; i < modelState.ModelTime.Days; i++)
        {
            if (modelState.Random() > hub.OperatingChance) continue;
            
            var operatingHour = await GetNewObjectsAsync(hub, TimeSpan.FromDays(i), cancellationToken);
            
            hub.OperatingHours.Add(operatingHour);
        }
    }
}