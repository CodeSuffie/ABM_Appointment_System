using Database;
using Database.Models;

namespace Repositories;

public sealed class BayShiftRepository(ModelDbContext context)
{
    public IQueryable<BayShift> Get()
    {
        return context.BayShifts;
    }
    
    public IQueryable<BayShift> Get(BayStaff bayStaff)
    {
        return Get()
            .Where(bs => bs.BayStaffId == bayStaff.Id);
    }
    
    public IQueryable<BayShift> Get(Bay bay)
    {
        return Get()
            .Where(bs => bs.BayId == bay.Id);
    }

    public async Task AddAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        await context.BayShifts.AddAsync(bayShift, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStartAsync(BayShift bayShift, TimeSpan startTime, CancellationToken cancellationToken)
    {
        bayShift.StartTime = startTime;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDurationAsync(BayShift bayShift, TimeSpan duration, CancellationToken cancellationToken)
    {
        bayShift.Duration = duration;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(BayShift bayShift, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        bayShift.BayStaff = bayStaff;
        bayStaff.Shifts.Remove(bayShift);
        bayStaff.Shifts.Add(bayShift);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(BayShift bayShift, Bay bay, CancellationToken cancellationToken)
    {
        bayShift.Bay = bay;

        await context.SaveChangesAsync(cancellationToken);
    }
}