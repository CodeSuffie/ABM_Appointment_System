using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AppointmentSlotRepository(ModelDbContext context)
{
    public IQueryable<AppointmentSlot> Get()
    {
        var appointmentSlots = context.AppointmentSlots;

        return appointmentSlots;
    }

    public IQueryable<AppointmentSlot> Get(Hub hub)
    {
        var appointmentSlots = Get()
            .Where(aps => aps.HubId == hub.Id);

        return appointmentSlots;
    }
    
    public IQueryable<AppointmentSlot> GetAfter(Hub hub, TimeSpan time)
    {
        var appointmentSlots = Get(hub)
            .Where(aps => aps.StartTime >= time);

        return appointmentSlots;
    }
    
    public async Task<AppointmentSlot?> GetAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        var appointmentSlot = await Get()
            .FirstOrDefaultAsync(aps => aps.Id == appointment.AppointmentSlotId, cancellationToken);

        return appointmentSlot;
    }
    
    public async Task AddAsync(AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        await context.AppointmentSlots
            .AddAsync(appointmentSlot, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(AppointmentSlot appointmentSlot, Hub hub, CancellationToken cancellationToken)
    {
        appointmentSlot.Hub = hub;
        hub.AppointmentSlots.Remove(appointmentSlot);
        hub.AppointmentSlots.Add(appointmentSlot);

        await context.SaveChangesAsync(cancellationToken);
    }
}