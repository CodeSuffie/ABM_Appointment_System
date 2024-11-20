using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.BayStaffServices;
using Services.BayServices;
using Settings;

namespace Services;

public sealed class BayShiftService(
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
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
        Hub hub,
        CancellationToken cancellationToken)
    {
        var bay = await bayService.SelectBayByHubAsync(hub, cancellationToken);
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
        var hub = await hubRepository.GetHubByStaffAsync(bayStaff, cancellationToken);
        var operatingHours = (await operatingHourRepository.GetOperatingHoursByHubAsync(hub, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            if (operatingHour.Duration == null) continue;
            
            if (ModelConfig.Random.NextDouble() >
                await bayStaffService.GetWorkChanceAsync(bayStaff, cancellationToken)) continue;

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, hub, cancellationToken);
            bayStaff.Shifts.Add(bayShift);
        }
    }
}