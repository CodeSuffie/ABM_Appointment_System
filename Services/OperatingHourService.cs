using Database.Models;
using Microsoft.Extensions.Logging;
using Services.ModelServices;

namespace Services;

public sealed class OperatingHourService(
    ILogger<OperatingHourService> logger,
    ModelState modelState)
{
    private TimeSpan? GetStartTime(Hub hub, TimeSpan day)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            hub.AverageOperatingHourLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Hub ({@Hub}) its OperatingHourLength ({TimeSpan}) is longer than a full day.",
                hub,
                hub.AverageOperatingHourLength);

            return null;
            // Hub Operating Hours can be longer than 1 day?
        }
        
        var operatingHourHour = modelState.Random(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

        return day + new TimeSpan(operatingHourHour, operatingHourMinutes, 0);
    }
    
    private OperatingHour? GetNewObject(Hub hub, TimeSpan day)
    {
        var startTime = GetStartTime(hub, day);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new OperatingHour for this Hub ({@Hub}).",
                hub);

            return null;
        }
        
        var operatingHour = new OperatingHour {
            Hub = hub,
            StartTime = (TimeSpan) startTime,
            Duration = hub.AverageOperatingHourLength,
        };

        return operatingHour;
    }

    public void GetNewObjects(Hub hub)
    {
        for (var i = 0; i < modelState.ModelTime.Days; i++)
        {
            var day = TimeSpan.FromDays(i);
            
            if (modelState.Random() > hub.OperatingChance)
            {
                logger.LogInformation("Hub ({@Hub}) will not have an OperatingHour during this day ({TimeSpan}).",
                    hub,
                    day);
                
                continue;
            }
            
            var operatingHour = GetNewObject(hub, day);
            if (operatingHour == null)
            {
                logger.LogError("No new OperatingHour could be created for this Hub ({@Hub}) during this day ({TimeSpan})",
                    hub,
                    day);

                continue;
            }
            
            hub.OperatingHours.Add(operatingHour);
            logger.LogInformation("New OperatingHour created for this Hub ({@Hub}) during this day ({TimeSpan}): OperatingHour={@OperatingHour}",
                hub,
                day,
                operatingHour);
        }
    }
}