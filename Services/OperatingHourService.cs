using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class OperatingHourService(
    ILogger<OperatingHourService> logger,
    HubRepository hubRepository,
    ModelState modelState)
{
    private TimeSpan? GetStartTime(Hub hub, TimeSpan day)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            hub.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Hub \n({@Hub})\n its OperatingHourLength \n({TimeSpan})\n is longer than a full day.",
                hub,
                hub.AverageShiftLength);

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
            logger.LogError("No start time could be assigned to the new OperatingHour for this Hub \n({@Hub})",
                hub);

            return null;
        }
        
        var operatingHour = new OperatingHour {
            Hub = hub,
            StartTime = (TimeSpan) startTime,
            Duration = hub.AverageShiftLength,
        };

        return operatingHour;
    }

    public async Task GetNewObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.ModelConfig.ModelTime.Days; i++)
        {
            var day = TimeSpan.FromDays(i);
            
            if (modelState.RandomDouble() > hub.WorkChance)
            {
                logger.LogInformation("Hub \n({@Hub})\n will not have an OperatingHour during this day \n({TimeSpan})",
                    hub,
                    day);
                
                continue;
            }
            
            var operatingHour = GetNewObject(hub, day);
            if (operatingHour == null)
            {
                logger.LogError("No new OperatingHour could be created for this Hub \n({@Hub})\n during this day \n({TimeSpan})\n",
                    hub,
                    day);

                continue;
            }

            await hubRepository.AddAsync(hub, operatingHour, cancellationToken);
            logger.LogInformation("New OperatingHour created for this Hub \n({@Hub})\n during this day \n({TimeSpan})\n: OperatingHour={@OperatingHour}",
                hub,
                day,
                operatingHour);
        }
    }
}