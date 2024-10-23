using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class HubService(ModelDbContext context) : IAgentService<Hub>
{
    private readonly ModelDbContext _context = context;
    
    public async Task InitializeAgentOperatingHourAsync(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.HubAverageOperatingDays) return;
            
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            AgentConfig.OperatingHourAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) return;      // Hub Operating Hours are longer than 1 day?
            
        var operatingHourHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var operatingHour = new OperatingHour {
            Hub = hub,
            StartTime = startTime + new TimeSpan(
                operatingHourHour,
                operatingHourMinutes,
                0
            ),
            Duration = AgentConfig.OperatingHourAverageLength,
        };
            
        hub.OperatingHours.Add(operatingHour);
    }

    public async Task InitializeAgentOperatingHoursAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ModelConfig.ModelTime.Days; i++)
        {
            await InitializeAgentOperatingHourAsync(hub, TimeSpan.FromDays(i), cancellationToken);
        }
    }
    
    public async Task InitializeAgentParkingSpotAsync(Hub hub, CancellationToken cancellationToken)
    {
        var parkingSpot = new ParkingSpot {
            Hub = hub
        };
        
        // TODO: Add Location
            
        hub.ParkingSpots.Add(parkingSpot);
    }

    public async Task InitializeAgentParkingSpotsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.ParkingSpotCountPerHub; i++)
        {
            await InitializeAgentParkingSpotAsync(hub, cancellationToken);
        }
    }
    
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub();
        
        // TODO: Add Location
        
        await InitializeAgentOperatingHoursAsync(hub, cancellationToken);
        await InitializeAgentParkingSpotsAsync(hub, cancellationToken);
        
        context.Hubs.Add(hub);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.HubCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Hub hub, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var hubs = await _context.Hubs.ToListAsync(cancellationToken);
        foreach (var hub in hubs)
        {
            await ExecuteStepAsync(hub, cancellationToken);
        }
    }
}
