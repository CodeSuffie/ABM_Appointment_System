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
    
    public IQueryable<HubShift> Get(Picker picker)
    {
        var shifts = context.HubShifts
            .Where(a => a.PickerId == picker.Id);

        return shifts;
    }
    
    public IQueryable<HubShift> Get(Stuffer stuffer)
    {
        var shifts = context.HubShifts
            .Where(a => a.StufferId == stuffer.Id);

        return shifts;
    }
}