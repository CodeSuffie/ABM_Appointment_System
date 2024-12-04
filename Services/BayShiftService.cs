using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.ModelServices;

namespace Services;

public sealed class BayShiftService(
    ILogger<BayShiftService> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    BayService bayService,
    BayStaffRepository bayStaffRepository,
    BayShiftRepository bayShiftRepository,
    ModelState modelState)
{

    private TimeSpan? GetStartTime(BayStaff bayStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - bayStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("BayStaff \n({@BayStaff})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan})",
                bayStaff,
                bayStaff.AverageShiftLength,
                operatingHour,
                operatingHour.Duration);

            return null;
        }

        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }
    
    public async Task<double?> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        
        if (hub != null) return bayStaff.WorkChance / hub.OperatingChance;
        
        logger.LogError("BayStaff \n({@BayStaff})\n did not have a Hub assigned to get the OperatingHourChance for.",
            bayStaff);

        return null;
    }
    
    public async Task<BayShift?> GetNewObjectAsync(
        BayStaff bayStaff, 
        OperatingHour operatingHour,
        Hub hub,
        CancellationToken cancellationToken)
    {
        var bay = await bayService.SelectBayAsync(hub, cancellationToken);
        if (bay == null)
        {
            logger.LogError("The Hub \n({@Hub})\n did not have a Bay to assign " +
                            "to the new BayShift for this BayStaff \n({@AdminStaff})",
                hub,
                bayStaff);

            return null;
        }
        
        var startTime = GetStartTime(bayStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new BayShift for this " +
                            "BayStaff \n({@BayStaff})\n during this OperatingHour \n({@OperatingHour})",
                bayStaff,
                operatingHour);

            return null;
        }
        
        var bayShift = new BayShift {
            BayStaff = bayStaff,
            Bay = bay,
            StartTime = (TimeSpan) startTime,
            Duration = bayStaff.AverageShiftLength,
        };

        return bayShift;
    }

    public async Task GetNewObjectsAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("BayStaff \n({@BayStaff})\n did not have a Hub assigned to create BayShifts for.",
                bayStaff);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(bayStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this BayStaff " +
                                "\n({@BayStaff})\n during this OperatingHour \n({@OperatingHour})",
                    bayStaff,
                    operatingHour);

                continue;
            }

            
            if (modelState.Random() > workChance)
            {
                logger.LogInformation("BayStaff \n({@BayStaff})\n will not have a BayShift during " +
                                      "this OperatingHour \n({@OperatingHour})",
                    bayStaff,
                    operatingHour);
                
                continue;
            }

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, hub, cancellationToken);
            if (bayShift == null)
            {
                logger.LogError("No new BayShift could be created for this BayStaff " +
                                "\n({@BayStaff})\n during this OperatingHour \n({@OperatingHour})\n",
                    bayStaff,
                    operatingHour);

                continue;
            }

            await bayStaffRepository.AddAsync(bayStaff, bayShift, cancellationToken);
            logger.LogInformation("New BayShift created for this BayStaff \n({@BayStaff})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: BayShift={@BayShift}",
                bayStaff,
                operatingHour,
                bayShift);
        }
    }
    
    private bool IsCurrent(BayShift bayShift)
    {
        var endTime = bayShift.StartTime + bayShift.Duration;
        
        return modelState.ModelTime >= bayShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<BayShift?> GetCurrentAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = bayShiftRepository.Get(bayStaff)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("BayShift \n({@BayShift})\n is currently active.",
                shift);
                
            return shift;
        }

        logger.LogInformation("No BayShift is currently active.");
        return null;
    }
}