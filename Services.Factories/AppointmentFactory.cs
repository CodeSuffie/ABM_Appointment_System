using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class AppointmentFactory(
    ILogger<AppointmentFactory> logger,
    AppointmentRepository appointmentRepository,
    ModelState modelState) : IFactoryService<Appointment>
{
    public async Task<Appointment?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var appointment = new Appointment();
        
        await appointmentRepository.AddAsync(appointment, cancellationToken);

        return appointment;
    }
    
    public async Task<Appointment?> GetNewObjectAsync(AppointmentSlot appointmentSlot, Bay bay, Trip trip, CancellationToken cancellationToken)
    {
        var appointment = await GetNewObjectAsync(cancellationToken);
        if (appointment == null)
        {
            logger.LogError("Appointment could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this AppointmentSlot \n({@AppointmentSlot})",
            appointment,
            appointmentSlot);
        await appointmentRepository.SetAsync(appointment, appointmentSlot, cancellationToken);

        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this Bay \n({@Bay})",
            appointment,
            bay);
        await appointmentRepository.SetAsync(appointment, bay, cancellationToken);
        
        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this Trip \n({@Trip})",
            appointment,
            trip);
        await appointmentRepository.SetAsync(appointment, trip, cancellationToken);

        return appointment;
    }
}