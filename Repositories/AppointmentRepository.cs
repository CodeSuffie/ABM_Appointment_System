using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AppointmentRepository(ModelDbContext context)
{
    public IQueryable<Appointment> Get()
    {
        return context.Appointments
            .Include(ap => ap.AppointmentSlot);
    }

    public IQueryable<Appointment> Get(AppointmentSlot appointmentSlot)
    {
        return Get()
            .Where(ap => ap.AppointmentSlotId == appointmentSlot.Id);
    }

    public IQueryable<Appointment> Get(Bay bay)
    {
        return Get()
            .Where(ap => ap.BayId == bay.Id);
    }
    
    public Task<Appointment?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(ap => ap.TripId == trip.Id, cancellationToken);
    }

    public Task<Appointment?> GetAsync(Bay bay, AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        return Get(appointmentSlot)
            .FirstOrDefaultAsync(ap => ap.BayId == bay.Id, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await context.Appointments.AddAsync(appointment, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(Appointment appointment, AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        appointment.AppointmentSlot = appointmentSlot;
        appointmentSlot.Appointments.Remove(appointment);
        appointmentSlot.Appointments.Add(appointment);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(Appointment appointment, Bay bay, CancellationToken cancellationToken)
    {
        appointment.Bay = bay;
        bay.Appointments.Remove(appointment);
        bay.Appointments.Add(appointment);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetAsync(Appointment appointment, Trip trip, CancellationToken cancellationToken)
    {
        appointment.Trip = trip;
        trip.Appointment = appointment;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        return Get(appointmentSlot)
            .CountAsync(cancellationToken);
    }
}