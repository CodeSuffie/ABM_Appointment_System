using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class AdminShiftService(ModelDbContext context)
{
    private static double GetAdminStaffWorkChance(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        return AgentConfig.AdminStaffAverageWorkDays / AgentConfig.HubAverageOperatingDays;
    }

    private static AdminShift? GetNewObject(AdminStaff adminStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            AgentConfig.AdminShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return null;
            
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
    
    public static async Task InitializeObjectAsync(AdminStaff adminStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        if (operatingHour.Duration == null) return;
            
        if (ModelConfig.Random.NextDouble() >
            GetAdminStaffWorkChance(adminStaff, cancellationToken)) return;

        var adminShift = GetNewObject(adminStaff, operatingHour, cancellationToken);
        if (adminShift != null)
        {
            adminStaff.Shifts.Add(adminShift);
        }
    }

    public async Task InitializeObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours.Where(
            x => x.HubId == adminStaff.Hub.Id
        ).ToList();
        
        foreach (var operatingHour in operatingHours)
        {
            await InitializeObjectAsync(adminStaff, operatingHour, cancellationToken);
        }
    }
}