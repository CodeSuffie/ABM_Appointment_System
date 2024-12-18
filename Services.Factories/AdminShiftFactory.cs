using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class AdminShiftFactory(
    ILogger<AdminShiftFactory> logger,
    HubRepository hubRepository,
    HubShiftRepository hubShiftRepository,
    OperatingHourRepository operatingHourRepository,
    ModelState modelState) : IShiftFactoryService<AdminStaff, HubShift, OperatingHour>
{
    public TimeSpan? GetStartTime(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - adminStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan}).",
                adminStaff,
                adminStaff.AverageShiftLength,
                operatingHour,
                operatingHour.Duration);

            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }

    public async Task<double?> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        
        if (hub != null) return adminStaff.WorkChance / hub.WorkChance;
        
        logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to get the OperatingHourChance for.",
            adminStaff);

        return null;
    }
    
    public async Task<HubShift?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hubShift = new HubShift();

        await hubShiftRepository.AddAsync(hubShift, cancellationToken);

        return hubShift;
    }

    public async Task<HubShift?> GetNewObjectAsync(AdminStaff adminStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var startTime = GetStartTime(adminStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this " +
                            "AdminStaff \n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour}).",
                adminStaff,
                operatingHour);

            return null;
        }

        var hubShift = await GetNewObjectAsync(cancellationToken);
        if (hubShift == null)
        {
            logger.LogError("HubShift could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting this StartTime ({Step}) for this HubShift \n({@HubShift}).",
            startTime,
            hubShift);
        await hubShiftRepository.SetStartAsync(hubShift, (TimeSpan) startTime, cancellationToken);
        
        logger.LogDebug("Setting this Duration ({Step}) for this HubShift \n({@HubShift}).",
            adminStaff.AverageShiftLength,
            hubShift);
        await hubShiftRepository.SetDurationAsync(hubShift, adminStaff.AverageShiftLength, cancellationToken);
        
        logger.LogDebug("Setting this AdminStaff \n({@AdminStaff})\n for this HubShift \n({@HubShift}).",
            adminStaff,
            hubShift);
        await hubShiftRepository.SetAsync(hubShift, adminStaff, cancellationToken);
        
        return hubShift;
    }

    public async Task GetNewObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to create HubShifts for.",
                adminStaff);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(adminStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this AdminStaff " +
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour}).",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("AdminStaff \n({@AdminStaff})\n will not have an HubShift during " +
                                      "this OperatingHour \n({@OperatingHour}).",
                    adminStaff,
                    operatingHour);
                
                continue;
            }
            
            var hubShift = await GetNewObjectAsync(adminStaff, operatingHour, cancellationToken);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this AdminStaff " +
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour}).",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            logger.LogInformation("New HubShift created for this AdminStaff \n({@AdminStaff})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}",
                adminStaff,
                operatingHour,
                hubShift);
        }
    }
}