using Database;
using Database.Models;

namespace Repositories;

public sealed class BayShiftRepository(ModelDbContext context)
{
    public IQueryable<BayShift> Get(BayStaff bayStaff)
    {
        var shifts = context.BayShifts
            .Where(bs => bs.BayStaffId == bayStaff.Id);

        return shifts;
    }
    
    public IQueryable<BayShift> Get(Bay bay)
    {
        var shifts = context.BayShifts
            .Where(bs => bs.BayId == bay.Id);

        return shifts;
    }

    public async Task AddAsync(BayShift bayShift, CancellationToken cancellationToken)
    {
        await context.BayShifts.AddAsync(bayShift, cancellationToken);

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