using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.AdminStaffServices;
using Settings;

namespace Services;

public sealed class AdminShiftService(
    AdminStaffService adminStaffService,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository) 
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
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

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
        var hub = await hubRepository.GetHubByStaffAsync(adminStaff, cancellationToken);
        var operatingHours = (await operatingHourRepository.GetOperatingHoursByHubAsync(hub, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null) continue;
            
            if (ModelConfig.Random.NextDouble() >
                await adminStaffService.GetWorkChanceAsync(adminStaff, cancellationToken)) continue;
            
            var adminShift = await GetNewObjectAsync(adminStaff, operatingHour, cancellationToken);
            
            adminStaff.Shifts.Add(adminShift);
        }
    }
}