using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class AdminShiftService(
    ILogger<AdminShiftService> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AdminStaffRepository adminStaffRepository,
    AdminShiftRepository adminShiftRepository,
    ModelState modelState) 
{
    private TimeSpan? GetStartTime(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - adminStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan})",
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
        
        if (hub != null) return adminStaff.WorkChance / hub.OperatingChance;
        
        logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to get the OperatingHourChance for.",
            adminStaff);

        return null;
    }
    
    public AdminShift? GetNewObject(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var startTime = GetStartTime(adminStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new AdminShift for this " +
                            "AdminStaff \n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
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
            logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to create AdminShifts for.",
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
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            if (modelState.Random() > workChance)
            {
                logger.LogInformation("AdminStaff \n({@AdminStaff})\n will not have an AdminShift during " +
                                      "this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);
                
                continue;
            }
            
            var adminShift = GetNewObject(adminStaff, operatingHour);
            if (adminShift == null)
            {
                logger.LogError("No new AdminShift could be created for this AdminStaff " +
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);

                continue;
            }

            await adminStaffRepository.AddAsync(adminStaff, adminShift, cancellationToken);
            logger.LogInformation("New AdminShift created for this AdminStaff \n({@AdminStaff})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: AdminShift={@AdminShift}",
                adminStaff,
                operatingHour,
                adminShift);
        }
    }
    
    private bool IsCurrent(AdminShift adminShift)
    {
        var endTime = adminShift.StartTime + adminShift.Duration;
        
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
            
            logger.LogInformation("AdminShift \n({@AdminShift})\n is currently active.",
                shift);
                
            return shift;
        }

        logger.LogInformation("No AdminShift is currently active.");
        return null;
    }
}