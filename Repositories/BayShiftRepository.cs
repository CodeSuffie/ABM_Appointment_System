using Database;
using Database.Models;

namespace Repositories;

public sealed class BayShiftRepository(ModelDbContext context)
{
    public Task<IQueryable<BayShift>> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(bs => bs.BayStaffId == bayStaff.Id);

        return Task.FromResult(shifts);
    }
    
    public Task<IQueryable<BayShift>> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(bs => bs.BayId == bay.Id);

        return Task.FromResult(shifts);
    }
}