using Database;
using Database.Models;

namespace Repositories;

public sealed class HubShiftRepository(ModelDbContext context)
{
    public IQueryable<HubShift> Get(AdminStaff adminStaff)
    {
        var shifts = context.HubShifts
            .Where(a => a.AdminStaffId == adminStaff.Id);

        return shifts;
    }
}