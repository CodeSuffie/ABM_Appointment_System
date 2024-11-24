using Database;
using Database.Models;

namespace Repositories;

public sealed class AdminShiftRepository(ModelDbContext context)
{
    public IQueryable<AdminShift> Get(AdminStaff adminStaff)
    {
        var shifts = context.AdminShifts
            .Where(a => a.AdminStaffId == adminStaff.Id);

        return shifts;
    }
}