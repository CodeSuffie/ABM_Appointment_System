using Database;
using Database.Models;

namespace Repositories;

public sealed class AdminShiftRepository(ModelDbContext context)
{
    public Task<IQueryable<AdminShift>> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = context.AdminShifts
            .Where(a => a.AdminStaffId == adminStaff.Id);

        return Task.FromResult(shifts);
    }
}