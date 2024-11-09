using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.BayStaffServices;
using Services.BayServices;
using Settings;

namespace Services;

public sealed class BayShiftService(
    ModelDbContext context,
    BayStaffService bayStaffService,
    BayService bayService)
{

    private async Task<TimeSpan> GetStartTimeAsync(
        BayStaff bayStaff,
        OperatingHour operatingHour,
        CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            bayStaff.AverageShiftLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This BayStaff its BayShiftLength is longer than the Hub its OperatingHourLength.");
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        return operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0);
    }
    
    public async Task<BayShift> GetNewObjectAsync(
        BayStaff bayStaff, 
        OperatingHour operatingHour,
        CancellationToken cancellationToken)
    {
        var bay = await bayService.SelectBayByStaff(bayStaff, cancellationToken);
        var startTime = await GetStartTimeAsync(bayStaff, operatingHour, cancellationToken);
        
        var bayShift = new BayShift {
            BayStaff = bayStaff,
            Bay = bay,
            StartTime = startTime,
            Duration = bayStaff.AverageShiftLength,
        };

        return bayShift;
    }

    public async Task GetNewObjectsAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours
            .Where(x => x.HubId == bayStaff.Hub.Id)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null) continue;
            
            if (ModelConfig.Random.NextDouble() >
                bayStaffService.GetWorkChance(bayStaff, cancellationToken)) continue;

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, cancellationToken);
            bayStaff.Shifts.Add(bayShift);
        }
    }
}