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
}