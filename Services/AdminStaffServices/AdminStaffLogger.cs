using Database.Models;
using Database.Models.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffLogger(
    AdminStaffRepository adminStaffRepository,
    ModelState modelState)
{
    private AdminStaffLog GetLog(LogType logType, string description)
    {
        var log = new AdminStaffLog
        {
            LogType = logType,
            LogTime = modelState.ModelTime,
            Description = description,
        };

        return log;
    }

    public async Task LogAsync(AdminStaff adminStaff, LogType logType, string description, CancellationToken cancellationToken)
    {
        var log = GetLog(logType, description);

        await adminStaffRepository.AddAsync(adminStaff, log, cancellationToken);
    }
}