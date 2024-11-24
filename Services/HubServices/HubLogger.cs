using Database.Models;
using Database.Models.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.HubServices;

public sealed class HubLogger(
    HubRepository hubRepository,
    ModelState modelState)
{
    private HubLog GetLog(LogType logType, string description)
    {
        var log = new HubLog
        {
            LogType = logType,
            LogTime = modelState.ModelTime,
            Description = description,
        };

        return log;
    }

    public async Task LogAsync(Hub hub, ParkingSpot parkingSpot, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"ParkingSpot with ID {parkingSpot.Id} logged: [{description}]";
        var log = GetLog(logType, description);

        await hubRepository.AddAsync(hub, log, cancellationToken);
    }
    
    public async Task LogAsync(Hub hub, AdminStaff adminStaff, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"AdminStaff with ID {adminStaff.Id} logged: [{description}]";
        var log = GetLog(logType, description);

        await hubRepository.AddAsync(hub, log, cancellationToken);
    }
    
    public async Task LogAsync(Hub hub, Bay bay, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"Bay with ID {bay.Id} logged: [{description}]";
        var log = GetLog(logType, description);

        await hubRepository.AddAsync(hub, log, cancellationToken);
    }
}