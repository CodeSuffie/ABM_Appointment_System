using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class OperatingHourService(ModelDbContext context)
{
    private async Task<OperatingHour> GetNewObject(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var maxShiftStart = TimeSpan.FromDays(1) - 
                            AgentConfig.OperatingHourAverageLength;
            
        if (maxShiftStart < TimeSpan.Zero) 
            throw new Exception("This Hub its OperatingHourLength is longer than a full day.");      
            // Hub Operating Hours can be longer than 1 day?
            
        var operatingHourHour = ModelConfig.Random.Next(maxShiftStart.Hours);
        var operatingHourMinutes = operatingHourHour == maxShiftStart.Hours ?
            ModelConfig.Random.Next(maxShiftStart.Minutes) :
            ModelConfig.Random.Next(ModelConfig.MinutesPerHour);

        var operatingHour = new OperatingHour {
            Hub = hub,
            StartTime = startTime + new TimeSpan(operatingHourHour, operatingHourMinutes, 0),
            Duration = AgentConfig.OperatingHourAverageLength,
        };

        return operatingHour;
    }
    
    public async Task InitializeObjectAsync(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        if (ModelConfig.Random.NextDouble() >
            AgentConfig.HubAverageOperatingDays) return;

        var operatingHour = await GetNewObject(hub, startTime, cancellationToken);
        hub.OperatingHours.Add(operatingHour);
    }

    public async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        for (var i = 0; i < ModelConfig.ModelTime.Days; i++)
        {
            await InitializeObjectAsync(hub, TimeSpan.FromDays(i), cancellationToken);
        }
    }
}