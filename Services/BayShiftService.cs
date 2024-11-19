using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.BayStaffServices;
using Services.BayServices;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class BayShiftService(
    ModelDbContext context,
    BayStaffService bayStaffService,
    BayService bayService,
    ModelService modelService)
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
                await bayStaffService.GetWorkChanceAsync(bayStaff, cancellationToken)) continue;

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, cancellationToken);
            bayStaff.Shifts.Add(bayShift);
        }
    }

    public async Task<bool> IsCurrentShiftAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var modelTime = await modelService.GetModelTimeAsync(cancellationToken);
        
        if (bayShift.Duration == null)
            throw new Exception("The shift for this BayStaff does not have a Duration.");
            
        var endTime = (TimeSpan)(bayShift.StartTime + bayShift.Duration);
        
        return modelTime >= bayShift.StartTime && modelTime <= endTime;
    }
    
    public async Task<BayShift?> GetCurrentBayShiftForBayStaffAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = (await bayStaffService.GetShiftsForBayStaffAsync(bayStaff, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (await IsCurrentShiftAsync(shift, cancellationToken))
            {
                return shift;
            }
        }

        return null;
    }

    public async Task<Bay?> GetBayForBayShiftAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        var bay = await context.Bays
            .FirstOrDefaultAsync(x => x.Id == bayShift.BayId, cancellationToken);
        
        return bay;
    }
}