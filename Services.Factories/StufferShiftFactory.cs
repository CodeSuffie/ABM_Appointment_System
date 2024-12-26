using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public class StufferShiftFactory(
    ILogger<StufferShiftFactory> logger,
    HubRepository hubRepository,
    HubShiftRepository hubShiftRepository,
    OperatingHourRepository operatingHourRepository,
    ModelState modelState) : IShiftFactoryService<Stuffer, HubShift, OperatingHour>
{
    public TimeSpan? GetStartTime(Stuffer stuffer, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - stuffer.AverageShiftLength;
    
        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its ShiftLength \n({TimeSpan})\n is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan}).", stuffer, stuffer.AverageShiftLength, operatingHour, operatingHour.Duration);
    
            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);
    
        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }

    public async Task<double?> GetWorkChanceAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        
        if (hub != null) return stuffer.WorkChance / hub.WorkChance;
        
        logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to get the OperatingHourChance for.", stuffer);
    
        return null;
    }
    
    public async Task<HubShift?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hubShift = new HubShift();

        await hubShiftRepository.AddAsync(hubShift, cancellationToken);

        return hubShift;
    }

    public async Task<HubShift?> GetNewObjectAsync(Stuffer stuffer, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var startTime = GetStartTime(stuffer, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this Stuffer \n({@Stuffer})\n during this OperatingHour \n({@OperatingHour}).", stuffer, operatingHour);

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
        
        logger.LogDebug("Setting this Duration ({Step}) for this HubShift \n({@HubShift}).", stuffer.AverageShiftLength, hubShift);
        await hubShiftRepository.SetDurationAsync(hubShift, stuffer.AverageShiftLength, cancellationToken);
        
        logger.LogDebug("Setting this Stuffer \n({@Stuffer})\n for this HubShift \n({@HubShift}).", stuffer, hubShift);
        await hubShiftRepository.SetAsync(hubShift, stuffer, cancellationToken);
        
        return hubShift;
    }

    public async Task GetNewObjectsAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to create HubShifts for.", stuffer);
    
            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(stuffer, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this Stuffer \n({@Stuffer})\n during this OperatingHour \n({@OperatingHour}).", stuffer, operatingHour);
    
                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("Stuffer \n({@Stuffer})\n will not have an HubShift during this OperatingHour \n({@OperatingHour}).", stuffer, operatingHour);
                
                continue;
            }
            
            var hubShift = await GetNewObjectAsync(stuffer, operatingHour, cancellationToken);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this Stuffer \n({@Stuffer})\n during this OperatingHour \n({@OperatingHour}).", stuffer, operatingHour);
    
                continue;
            }
            
            logger.LogInformation("New HubShift created for this Stuffer \n({@Stuffer})\n during this OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}", stuffer, operatingHour, hubShift);
        }
    }
}