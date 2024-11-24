using Database;
using Database.Models;

namespace Repositories;

public sealed class OperatingHourRepository(ModelDbContext context)
{
    public IQueryable<OperatingHour> Get(Hub hub)
    {
        var operatingHours = context.OperatingHours
            .Where(oh => oh.HubId == hub.Id);

        return operatingHours;
    }
}