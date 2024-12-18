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

    public async Task AddAsync(OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        await context.OperatingHours.AddAsync(operatingHour, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStartAsync(OperatingHour operatingHour, TimeSpan startTime, CancellationToken cancellationToken)
    {
        operatingHour.StartTime = startTime;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDurationAsync(OperatingHour operatingHour, TimeSpan duration, CancellationToken cancellationToken)
    {
        operatingHour.Duration = duration;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(OperatingHour operatingHour, Hub hub, CancellationToken cancellationToken)
    {
        operatingHour.Hub = hub;
        hub.Shifts.Remove(operatingHour);
        hub.Shifts.Add(operatingHour);

        await context.SaveChangesAsync(cancellationToken);
    }
}