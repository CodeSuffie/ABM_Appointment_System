using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.BayStaffServices;
using Services.BayServices;
using Services.ModelServices;
using Settings;

namespace Services;

public sealed class BayShiftService(
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    BayStaffService bayStaffService,
    BayService bayService,
    BayShiftRepository bayShiftRepository,
    ModelState modelState)
{

    private Task<TimeSpan> GetStartTimeAsync(
        BayStaff bayStaff,
        OperatingHour operatingHour,
        CancellationToken cancellationToken)
    {
        var maxShiftStart = operatingHour.Duration!.Value - 
                            bayStaff.AverageShiftLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This BayStaff its BayShiftLength is longer than the Hub its OperatingHourLength.");
            
        var shiftHour = modelState.Random(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            modelState.Random(maxShiftStart.Minutes) :
            modelState.Random(modelState.ModelConfig.MinutesPerHour);

        return Task.FromResult(operatingHour.StartTime + new TimeSpan(shiftHour, shiftMinutes, 0));
    }
    
    public async Task<BayShift> GetNewObjectAsync(
        BayStaff bayStaff, 
        OperatingHour operatingHour,
        Hub hub,
        CancellationToken cancellationToken)
    {
        var bay = await bayService.SelectBayAsync(hub, cancellationToken);
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
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null) continue;
            
            if (modelState.Random() >
                await bayStaffService.GetWorkChanceAsync(bayStaff, cancellationToken)) continue;

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, hub, cancellationToken);
            bayStaff.Shifts.Add(bayShift);
        }
    }
    
    private Task<bool> IsCurrentAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        if (bayShift.Duration == null)
            throw new Exception("The shift for this BayStaff does not have a Duration.");
            
        var endTime = (TimeSpan)(bayShift.StartTime + bayShift.Duration);
        
        return Task.FromResult(modelState.ModelTime >= bayShift.StartTime && modelState.ModelTime <= endTime);
    }
    
    public async Task<BayShift?> GetCurrentAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = bayShiftRepository.Get(bayStaff)
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
    
    public async Task<BayShift?> GetCurrentAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = bayShiftRepository.Get(bay)
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