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
    BayShiftRepository bayShiftRepository,
    ModelState modelState)
{

    private TimeSpan? GetStartTime(BayStaff bayStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration!.Value - bayStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("BayStaff ({@BayStaff}) its ShiftLength ({TimeSpan}) " +
                            "is longer than this OperatingHour ({@OperatingHour}) its Length ({TimeSpan}).",
                bayStaff,
                bayStaff.AverageShiftLength,
                operatingHour,
                operatingHour.Duration!.Value);

            return null;
        }
            
        var shiftHour = modelState.Random(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

        return operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0);
    }
    
    public async Task<double?> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        
        if (hub != null) return bayStaff.WorkChance / hub.OperatingChance;
        
        logger.LogError("BayStaff ({@BayStaff}) did not have a Hub assigned to get the OperatingHourChance for.",
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
            logger.LogError("The Hub ({@Hub}) did not have a Bay to assign " +
                            "to the new BayShift for this BayStaff ({@AdminStaff}).",
                hub,
                bayStaff);

            return null;
        }
        
        var startTime = GetStartTime(bayStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new BayShift for this " +
                            "BayStaff ({@BayStaff}) during this OperatingHour ({@OperatingHour}).",
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
            logger.LogError("BayStaff ({@BayStaff}) did not have a Hub assigned to create BayShifts for.",
                bayStaff);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null)
            {
                logger.LogError("OperatingHour ({@OperatingHour}) does not have a Duration.",
                    operatingHour);
                continue;
            }
            
            var workChance = await GetWorkChanceAsync(bayStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this BayStaff " +
                                "({@BayStaff}) during this OperatingHour ({@OperatingHour}).",
                    bayStaff,
                    operatingHour);

                continue;
            }

            
            if (modelState.Random() > workChance)
            {
                logger.LogInformation("BayStaff ({@BayStaff}) will not have a BayShift during " +
                                      "this OperatingHour ({@OperatingHour}).",
                    bayStaff,
                    operatingHour);
                
                continue;
            }

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, hub, cancellationToken);
            if (bayShift == null)
            {
                logger.LogError("No new BayShift could be created for this BayStaff " +
                                "({@BayStaff}) during this OperatingHour ({@OperatingHour})",
                    bayStaff,
                    operatingHour);

                continue;
            }
            
            bayStaff.Shifts.Add(bayShift);
            logger.LogInformation("New BayShift created for this BayStaff ({@BayStaff}) during this " +
                                  "OperatingHour ({@OperatingHour}): BayShift={@BayShift}",
                bayStaff,
                operatingHour,
                bayShift);
        }
    }
    
    private bool IsCurrent(BayShift bayShift)
    {
        if (bayShift.Duration == null)
        {
            logger.LogError("BayShift ({@BayShift}) does not have a Duration",
                bayShift);

            return false;
        }
            
        var endTime = (TimeSpan)(bayShift.StartTime + bayShift.Duration);
        
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
            
            logger.LogInformation("BayShift ({@BayShift}) is currently active.",
                shift);
                
            return shift;
        }

        logger.LogInformation("No BayShift is currently active.");
        return null;
    }
}