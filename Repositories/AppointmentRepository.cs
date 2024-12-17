using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AppointmentRepository(ModelDbContext context)
{
    public IQueryable<Appointment> Get()
    {
        var appointments = context.Appointments
            .Include(ap => ap.AppointmentSlot);

        return appointments;
    }

    public IQueryable<Appointment> Get(AppointmentSlot appointmentSlot)
    {
        var appointments = Get()
            .Where(ap => ap.AppointmentSlotId == appointmentSlot.Id);

        return appointments;
    }

    public IQueryable<Appointment> Get(Bay bay)
    {
        var appointments = Get()
            .Where(ap => ap.BayId == bay.Id);

        return appointments;
    }
    
    public Task<Appointment?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var appointmentSlot = Get()
            .FirstOrDefaultAsync(ap => ap.TripId == trip.Id, cancellationToken);

        return appointmentSlot;
    }

    public Task<Appointment?> GetAsync(Bay bay, AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        var appointment = Get(appointmentSlot)
            .FirstOrDefaultAsync(ap => ap.BayId == bay.Id, cancellationToken);

        return appointment;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await context.Appointments.AddAsync(appointment, cancellationToken);

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