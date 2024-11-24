using Database;
using Database.Models;

namespace Repositories;

public sealed class OperatingHourRepository(ModelDbContext context)
{
    public Task<IQueryable<OperatingHour>> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var operatingHours = context.OperatingHours
            .Where(oh => oh.HubId == hub.Id);

        return Task.FromResult(operatingHours);
    }
}