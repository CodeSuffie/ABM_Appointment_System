using Database.Models;
using Database.Models.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.BayServices;

public sealed class BayLogger(
    BayRepository bayRepository,
    ModelState modelState)
{
    private BayLog GetLog(LogType logType, string description)
    {
        var log = new BayLog
        {
            LogType = logType,
            LogTime = modelState.ModelTime,
            Description = description,
        };

        return log;
    }
    
    public async Task LogAsync(Bay bay, BayStatus bayStatus, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"The BayStatus {bayStatus} is: [ {description} ]";
        var log = GetLog(logType, fullDescription);
        
        await bayRepository.AddAsync(bay, log, cancellationToken);
    }

    public async Task LogAsync(Bay bay, Load load, LogType logType, string description, CancellationToken cancellationToken)
    {
        var fullDescription = $"Load with ID {load.Id} is: [ {description} ]";
        var log = GetLog(logType, fullDescription);
        
        await bayRepository.AddAsync(bay, log, cancellationToken);
    }
}