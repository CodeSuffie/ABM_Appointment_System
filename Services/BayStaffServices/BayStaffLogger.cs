using Database.Models;
using Database.Models.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.BayStaffServices;

public sealed class BayStaffLogger(
    BayStaffRepository bayStaffRepository,
    ModelState modelState)
{
    private BayStaffLog GetLog(LogType logType, string description)
    {
        var log = new BayStaffLog
        {
            LogType = logType,
            LogTime = modelState.ModelTime,
            Description = description,
        };

        return log;
    }

    public async Task LogAsync(BayStaff bayStaff, LogType logType, string description, CancellationToken cancellationToken)
    {
        var log = GetLog(logType, description);

        await bayStaffRepository.AddAsync(bayStaff, log, cancellationToken);
    }
}