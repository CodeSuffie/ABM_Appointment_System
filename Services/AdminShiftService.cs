using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.AdminStaffServices;
using Services.ModelServices;

namespace Services;

public sealed class AdminShiftService(
    ILogger<AdminShiftService> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AdminShiftRepository adminShiftRepository,
    ModelState modelState) 
{
    private TimeSpan? GetStartTime(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration!.Value - adminStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("AdminStaff ({@AdminStaff}) its ShiftLength ({TimeSpan}) " +
                            "is longer than this OperatingHour ({@OperatingHour}) its Length ({TimeSpan}).",
                adminStaff,
                adminStaff.AverageShiftLength,
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
    
    public async Task<double?> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        
        if (hub != null) return adminStaff.WorkChance / hub.OperatingChance;
        
        logger.LogError("AdminStaff ({@AdminStaff}) did not have a Hub assigned to get the OperatingHourChance for.",
            adminStaff);

        return null;
    }
    
    public AdminShift? GetNewObject(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var startTime = GetStartTime(adminStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new AdminShift for this " +
                            "AdminStaff ({@AdminStaff}) during this OperatingHour ({@OperatingHour}).",
                adminStaff,
                operatingHour);

            return null;
        }
        
        var adminShift = new AdminShift {
            AdminStaff = adminStaff,
            StartTime = (TimeSpan) startTime,
            Duration = adminStaff.AverageShiftLength
        };

        return adminShift;
    }

    public async Task GetNewObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("AdminStaff ({@AdminStaff}) did not have a Hub assigned to create AdminShifts for.",
                adminStaff);

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

            var workChance = await GetWorkChanceAsync(adminStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this AdminStaff " +
                                "({@AdminStaff}) during this OperatingHour ({@OperatingHour}).",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            if (modelState.Random() > workChance)
            {
                logger.LogInformation("AdminStaff ({@AdminStaff}) will not have an AdminShift during " +
                                      "this OperatingHour ({@OperatingHour}).",
                    adminStaff,
                    operatingHour);
                
                continue;
            }
            
            var adminShift = GetNewObject(adminStaff, operatingHour);
            if (adminShift == null)
            {
                logger.LogError("No new AdminShift could be created for this AdminStaff " +
                                "({@AdminStaff}) during this OperatingHour ({@OperatingHour}).",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            adminStaff.Shifts.Add(adminShift);
            logger.LogInformation("New AdminShift created for this AdminStaff ({@AdminStaff}) during this " +
                                  "OperatingHour ({@OperatingHour}): AdminShift={@AdminShift}",
                adminStaff,
                operatingHour,
                adminShift);
        }
    }
    
    private bool IsCurrent(AdminShift adminShift)
    {
        if (adminShift.Duration == null)
        {
            logger.LogError("AdminShift ({@AdminShift}) does not have a Duration",
                adminShift);

            return false;
        }
            
        var endTime = (TimeSpan)(adminShift.StartTime + adminShift.Duration);
        
        return modelState.ModelTime >= adminShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<AdminShift?> GetCurrentAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = adminShiftRepository.Get(adminStaff)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("AdminShift ({@AdminShift}) is currently active.",
                shift);
                
            return shift;
        }

        logger.LogInformation("No AdminShift is currently active.");
        return null;
    }
}