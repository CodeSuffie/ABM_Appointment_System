using Database.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

public sealed class WorkService(
    ILogger<WorkService> logger,
    ModelState modelState)
{
    // TODO: Add timer for Appointment mode for how long BayStaff is working
    
    public bool IsWorkCompleted(Work work)
    {
        if (work.Duration == null)
        {
            logger.LogError("Work \n({@Work})\n does not have a Duration",
                work);

            return false;
        }
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        
        return endTime <= modelState.ModelTime;
    }
}