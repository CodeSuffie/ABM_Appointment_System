using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class AppointmentRepository(ModelDbContext context)
{
    public IQueryable<Appointment> Get()
    {
        var appointments = context.Appointments;

        return appointments;
    }
    
    public async Task<Appointment?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var appointmentSlot = await Get()
            .FirstOrDefaultAsync(ap => ap.TripId == trip.Id, cancellationToken);

        return appointmentSlot;
    }
}