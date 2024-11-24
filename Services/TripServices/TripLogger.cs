using Database.Models;
using Database.Models.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.TripServices;

public sealed class TripLogger(
    TripRepository tripRepository,
    ModelState modelState)
{
    private TripLog GetLog(LogType logType, string description)
    {
        var log = new TripLog
        {
            LogType = logType,
            LogTime = modelState.ModelTime,
            Description = description,
        };

        return log;
    }
    
    public async Task LogAsync(Trip trip, LogType logType, string description, CancellationToken cancellationToken)
    {
        var log = GetLog(logType, description);
        
        await tripRepository.AddAsync(trip, log, cancellationToken);
    }

    public async Task LogAsync(Trip trip, Load load, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"Load with ID {load.Id} is: [ {description} ]";
        var log = GetLog(logType, fullDescription);
        
        await tripRepository.AddAsync(trip, log, cancellationToken);
    }
}