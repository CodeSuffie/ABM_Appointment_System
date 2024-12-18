using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class AppointmentFactory(
    ILogger<AppointmentFactory> logger,
    AppointmentRepository appointmentRepository,
    AppointmentSlotFactory appointmentSlotFactory,
    BayRepository bayRepository,
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
    
    public async Task<Bay?> GetVacantAsync(Hub hub, AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        var bays = bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var bay in bays)
        {
            logger.LogDebug("Getting Appointment for this Bay \n({@Bay})\n in this AppointmentSlot \n({@AppointmentSlot}).",
                bay,
                appointmentSlot);
            var appointment = await appointmentRepository.GetAsync(bay, appointmentSlot, cancellationToken);
            if (appointment == null)
            {
                logger.LogInformation("Bay \n({@Bay})\n does not have an Appointment assigned in this " +
                                      "AppointmentSlot \n({@AppointmentSlot}).",
                    bay,
                    appointmentSlot);

                if (bay.BayStatus != BayStatus.Closed) return bay;
                
                logger.LogInformation("Bay \n({@Bay})\n is closed during this AppointmentSlot \n({@AppointmentSlot}).",
                    bay,
                    appointmentSlot);

                continue;

            }
            
            logger.LogDebug("Bay \n({@Bay})\n had an appointment \n({@Appointment})\n in this AppointmentSlot \n({@AppointmentSlot}).",
                bay,
                appointment,
                appointmentSlot);
        }

        return null;
    }
    
    public async Task SetAsync(Trip trip, Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var appointmentSlot = await appointmentSlotFactory.GetNextVacantAsync(hub, startTime, cancellationToken);
        if (appointmentSlot == null)
        {
            logger.LogError("Hub \n({@Hub})\n did not have any AppointmentSlot with available Appointments after this " +
                            "Step \n({Step})\n for this Trip \n({@Trip}).",
                hub, 
                startTime, 
                trip);

            return;
        }

        var bay = await GetVacantAsync(hub, appointmentSlot, cancellationToken);
        if (bay == null)
        {
            logger.LogError("Hub \n({@Hub})\n with this AppointmentSlot \n({@AppointmentSlot})\n had no Bay " +
                            "available for an Appointment after this Step \n({Step})\n for this Trip \n({@Trip}).",
                hub, 
                appointmentSlot,
                startTime, 
                trip);

            return;
        }

        var appointment = await GetNewObjectAsync(appointmentSlot, bay, trip, cancellationToken);
        
        logger.LogInformation("New Appointment created: Appointment={@Appointment}", appointment);
    }
}