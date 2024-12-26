using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class PickerShiftFactory(
    ILogger<PickerShiftFactory> logger,
    HubRepository hubRepository,
    HubShiftRepository hubShiftRepository,
    OperatingHourRepository operatingHourRepository,
    ModelState modelState) : IShiftFactoryService<Picker, HubShift, OperatingHour>
{
    public TimeSpan? GetStartTime(Picker picker, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - picker.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Picker \n({@Picker})\n its ShiftLength \n({TimeSpan})\n is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan}).", picker, picker.AverageShiftLength, operatingHour, operatingHour.Duration);

            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }

    public async Task<double?> GetWorkChanceAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(picker, cancellationToken);
        
        if (hub != null) return picker.WorkChance / hub.WorkChance;
        
        logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to get the OperatingHourChance for.", picker);

        return null;
    }
    
    public async Task<HubShift?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hubShift = new HubShift();

        await hubShiftRepository.AddAsync(hubShift, cancellationToken);

        return hubShift;
    }

    public async Task<HubShift?> GetNewObjectAsync(Picker picker, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var startTime = GetStartTime(picker, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this Picker \n({@Picker})\n during this OperatingHour \n({@OperatingHour}).", picker, operatingHour);

            return null;
        }

        var hubShift = await GetNewObjectAsync(cancellationToken);
        if (hubShift == null)
        {
            logger.LogError("HubShift could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting this StartTime ({Step}) for this HubShift \n({@HubShift}).", startTime, hubShift);
        await hubShiftRepository.SetStartAsync(hubShift, (TimeSpan) startTime, cancellationToken);
        
        logger.LogDebug("Setting this Duration ({Step}) for this HubShift \n({@HubShift}).", picker.AverageShiftLength, hubShift);
        await hubShiftRepository.SetDurationAsync(hubShift, picker.AverageShiftLength, cancellationToken);
        
        logger.LogDebug("Setting this Picker \n({@Picker})\n for this HubShift \n({@HubShift}).", picker, hubShift);
        await hubShiftRepository.SetAsync(hubShift, picker, cancellationToken);
        
        return hubShift;
    }

    public async Task GetNewObjectsAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(picker, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to create HubShifts for.", picker);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(picker, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this Picker \n({@Picker})\n during this OperatingHour \n({@OperatingHour}).", picker, operatingHour);

                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("Picker \n({@Picker})\n will not have an HubShift during this OperatingHour \n({@OperatingHour}).", picker, operatingHour);
                
                continue;
            }
            
            var hubShift = await GetNewObjectAsync(picker, operatingHour, cancellationToken);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this Picker \n({@Picker})\n during this OperatingHour \n({@OperatingHour}).", picker, operatingHour);

                continue;
            }
            
            logger.LogInformation("New HubShift created for this Picker \n({@Picker})\n during this OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}", picker, operatingHour, hubShift);
        }
    }
}