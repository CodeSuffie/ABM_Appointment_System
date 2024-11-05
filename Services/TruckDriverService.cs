using System.Globalization;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckDriverService(ModelDbContext context) : IAgentService<TruckDriver>
{
    private static async Task InitializeAgentShiftAsync(TruckDriver truckDriver, TimeSpan startTime, CancellationToken cancellationToken)
    {
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.TruckDriverAverageWorkDays) return;
            
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            AgentConfig.TruckShiftAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return;      // Truck Drivers are working longer than 1 day shifts?
        
        // TODO: Add Trips
            
        var shiftHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var shiftMinutes = shiftHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var shift = new TruckShift {
            TruckDriver = truckDriver,
            StartTime = startTime + new TimeSpan(shiftHour, shiftMinutes, 0),
            Duration = AgentConfig.TruckShiftAverageLength,
        };
            
        truckDriver.Shifts.Add(shift);
    }

    private static async Task InitializeAgentShiftsAsync(TruckDriver truckDriver, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ModelConfig.ModelTime.Days; i++)
        {
            await InitializeAgentShiftAsync(truckDriver, TimeSpan.FromDays(i), cancellationToken);
        }
    }

    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies.ToList();
        var truckCompany = truckCompanies[ModelConfig.Random.Next(truckCompanies.Count)];
        
        var truckDriver = new TruckDriver
        {
            TruckCompany = truckCompany
        };
        
        await InitializeAgentShiftsAsync(truckDriver, cancellationToken);
        
        context.TruckDrivers.Add(truckDriver);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckDriverCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(TruckDriver truckDriver, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var truckDrivers = await context.TruckDrivers.ToListAsync(cancellationToken);
        foreach (var truckDriver in truckDrivers)
        {
            await ExecuteStepAsync(truckDriver, cancellationToken);
        }
    }
}
