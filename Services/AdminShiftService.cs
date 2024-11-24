using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.AdminStaffServices;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class AdminShiftService(
    AdminStaffService adminStaffService,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AdminShiftRepository adminShiftRepository,
    ModelState modelState) 
{
    private async Task<TimeSpan> GetStartTimeAsync(
        AdminStaff adminStaff,
        OperatingHour operatingHour,
        CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            adminStaff.AverageShiftLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This AdminStaff its AdminShiftLength is longer than the Hub its OperatingHourLength.");
            
        var shiftHour = modelState.Random(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

        return operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0);
    }
    
    public async Task<AdminShift> GetNewObjectAsync(
        AdminStaff adminStaff, 
        OperatingHour operatingHour, 
        CancellationToken cancellationToken)
    {
        var startTime = await GetStartTimeAsync(adminStaff, operatingHour, cancellationToken);
        
        var adminShift = new AdminShift {
            AdminStaff = adminStaff,
            StartTime = startTime,
            Duration = adminStaff.AverageShiftLength
        };

        return adminShift;
    }

    public async Task GetNewObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        var operatingHours = (await operatingHourRepository.GetAsync(hub, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null) continue;
            
            if (modelState.Random() >
                await adminStaffService.GetWorkChanceAsync(adminStaff, cancellationToken)) continue;
            
            var adminShift = await GetNewObjectAsync(adminStaff, operatingHour, cancellationToken);
            
            adminStaff.Shifts.Add(adminShift);
        }
    }
    
    private async Task<bool> IsCurrentAsync(AdminShift adminShift, CancellationToken cancellationToken)
    {
        if (adminShift.Duration == null)
            throw new Exception("The shift for this AdminStaff does not have a Duration.");
            
        var endTime = (TimeSpan)(adminShift.StartTime + adminShift.Duration);
        
        return modelState.ModelTime >= adminShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<AdminShift?> GetCurrentAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = (await adminShiftRepository.GetAsync(adminStaff, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (await IsCurrentAsync(shift, cancellationToken))
            {
                return shift;
            }
        }

        return null;
    }
}