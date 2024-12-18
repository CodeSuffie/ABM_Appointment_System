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

    public async Task AddAsync(HubShift hubShift, CancellationToken cancellationToken)
    {
        await context.HubShifts.AddAsync(hubShift, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetStartAsync(HubShift hubShift, TimeSpan startTime, CancellationToken cancellationToken)
    {
        hubShift.StartTime = startTime;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDurationAsync(HubShift hubShift, TimeSpan duration, CancellationToken cancellationToken)
    {
        hubShift.Duration = duration;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(HubShift hubShift, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        hubShift.AdminStaff = adminStaff;
        adminStaff.Shifts.Remove(hubShift);
        adminStaff.Shifts.Add(hubShift);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(HubShift hubShift, Picker picker, CancellationToken cancellationToken)
    {
        hubShift.Picker = picker;
        picker.Shifts.Remove(hubShift);
        picker.Shifts.Add(hubShift);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(HubShift hubShift, Stuffer stuffer, CancellationToken cancellationToken)
    {
        hubShift.Stuffer = stuffer;
        stuffer.Shifts.Remove(hubShift);
        stuffer.Shifts.Add(hubShift);

        await context.SaveChangesAsync(cancellationToken);
    }
}