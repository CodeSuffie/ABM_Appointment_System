using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Services;

public sealed class AdminShiftService(ModelDbContext context)
{
    private static double GetAdminStaffWorkChance(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        return AgentConfig.AdminStaffAverageWorkDays / AgentConfig.HubAverageOperatingDays;
    }

    private async Task<AdminShift> GetNewObject(
        AdminStaff adminStaff, 
        OperatingHour operatingHour, 
        CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            AgentConfig.AdminShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This AdminStaff its AdminShiftLength is longer than the Hub its OperatingHourLength.");
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var adminShift = new AdminShift {
            AdminStaff = adminStaff,
            StartTime = operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.AdminShiftAverageLength
        };

        return adminShift;
    }
    
    public async Task InitializeObjectAsync(
        AdminStaff adminStaff, 
        OperatingHour operatingHour, 
        CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            GetAdminStaffWorkChance(adminStaff, cancellationToken)) return;

        var adminShift = await GetNewObject(adminStaff, operatingHour, cancellationToken);
        adminStaff.Shifts.Add(adminShift);
    }

    public async Task InitializeObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours
            .Where(x => x.HubId == adminStaff.Hub.Id)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            await InitializeObjectAsync(adminStaff, operatingHour, cancellationToken);
        }
    }

    public Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}