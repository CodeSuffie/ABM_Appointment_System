using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AppointmentSlotRepository(ModelDbContext context)
{
    public IQueryable<AppointmentSlot> Get()
    {
        return context.AppointmentSlots;
    }

    public IQueryable<AppointmentSlot> Get(Hub hub)
    {
        return Get()
            .Where(aps => aps.HubId == hub.Id);
    }
    
    public IQueryable<AppointmentSlot> GetAfter(Hub hub, TimeSpan time)
    {
        return Get(hub)
            .Where(aps => aps.StartTime >= time);
    }

    public IQueryable<AppointmentSlot> GetBetween(Hub hub, TimeSpan startTime, TimeSpan endTime, TimeSpan duration)
    {
        return Get(hub)
            .Where(aps => aps.StartTime + duration >= startTime && aps.StartTime <= endTime);
    }
    
    public Task<AppointmentSlot?> GetAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(aps => aps.Id == appointment.AppointmentSlotId, cancellationToken);
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

    public async Task SetAsync(AppointmentSlot appointmentSlot, TimeSpan startTime, CancellationToken cancellationToken)
    {
        appointmentSlot.StartTime = startTime;

        await context.SaveChangesAsync(cancellationToken);
    }
}