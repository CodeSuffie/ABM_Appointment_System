using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class OperatingHourFactory(
    ILogger<OperatingHourFactory> logger,
    OperatingHourRepository operatingHourRepository,
    HubRepository hubRepository,
    ModelState modelState) : IShiftFactoryService<Hub, OperatingHour, TimeSpan>
{
    public TimeSpan? GetStartTime(Hub hub, TimeSpan day)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            hub.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Hub \n({@Hub})\n its OperatingHourLength \n({TimeSpan})\n is longer than a full day.", hub, hub.AverageShiftLength);

            return null;
            // Hub Operating Hours can be longer than 1 day?
        }
        
        var operatingHourHour = modelState.Random(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

        return day + new TimeSpan(operatingHourHour, operatingHourMinutes, 0);
    }

    public async Task<double?> GetWorkChanceAsync(Hub hub, CancellationToken cancellationToken)
    {
        return hub.WorkChance;
    }
    
    public async Task<OperatingHour?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var operatingHour = new OperatingHour();

        await operatingHourRepository.AddAsync(operatingHour, cancellationToken);

        return operatingHour;
    }

    public async Task<OperatingHour?> GetNewObjectAsync(Hub hub, TimeSpan day, CancellationToken cancellationToken)
    {
        var startTime = GetStartTime(hub, day);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new OperatingHour for this Hub \n({@Hub}).", hub);

            return null;
        }

        var operatingHour = await GetNewObjectAsync(cancellationToken);
        if (operatingHour == null)
        {
            logger.LogError("OperatingHour could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting this StartTime ({Step}) for this OperatingHour \n({@OperatingHour}).", startTime, operatingHour);
        await operatingHourRepository.SetStartAsync(operatingHour, (TimeSpan) startTime, cancellationToken);
        
        logger.LogDebug("Setting this Duration ({Step}) for this OperatingHour \n({@OperatingHour}).", hub.AverageShiftLength, operatingHour);
        await operatingHourRepository.SetDurationAsync(operatingHour, hub.AverageShiftLength, cancellationToken);
        
        logger.LogDebug("Setting this Hub \n({@Hub})\n for this OperatingHour \n({@OperatingHour}).", hub, operatingHour);
        await operatingHourRepository.SetAsync(operatingHour, hub, cancellationToken);
        
        return operatingHour;
    }

    public async Task GetNewObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i <= modelState.ModelConfig.ModelTotalTime.Days; i++)
        {
            var day = TimeSpan.FromDays(i);
            
            if (modelState.RandomDouble() > hub.WorkChance)
            {
                logger.LogInformation("Hub \n({@Hub})\n will not have an OperatingHour during this day \n({TimeSpan})", hub, day);
                
                continue;
            }
            
            var operatingHour = await GetNewObjectAsync(hub, day, cancellationToken);
            if (operatingHour == null)
            {
                logger.LogError("No new OperatingHour could be created for this Hub \n({@Hub})\n during this day \n({TimeSpan})\n", hub, day);

                continue;
            }

            await hubRepository.AddAsync(hub, operatingHour, cancellationToken);
            logger.LogInformation("New OperatingHour created for this Hub \n({@Hub})\n during this day \n({TimeSpan})\n: OperatingHour={@OperatingHour}", hub, day, operatingHour);
        }
    }
}