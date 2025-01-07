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
    AppointmentSlotRepository appointmentSlotRepository,
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
        
        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this AppointmentSlot \n({@AppointmentSlot})", appointment, appointmentSlot);
        await appointmentRepository.SetAsync(appointment, appointmentSlot, cancellationToken);

        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this Bay \n({@Bay})", appointment, bay);
        await appointmentRepository.SetAsync(appointment, bay, cancellationToken);
        
        logger.LogDebug("Setting Appointment \n({@Appointment})\n to this Trip \n({@Trip})", appointment, trip);
        await appointmentRepository.SetAsync(appointment, trip, cancellationToken);

        return appointment;
    }
    
    public async Task<Bay?> GetVacantAsync(Hub hub, AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
    {
        var bays = bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var slotLength = modelState.AppointmentConfig!.AppointmentLength * 
                         modelState.ModelConfig.ModelStep;
        var blockingCount = (modelState.AppointmentConfig!.AppointmentLength /
                             modelState.AppointmentConfig!.AppointmentSlotDifference) - 1;
        var slotDifference = modelState.AppointmentConfig!.AppointmentSlotDifference *
                             modelState.ModelConfig.ModelStep;
        var startTime = appointmentSlot.StartTime - blockingCount * slotDifference;
        var endTime = appointmentSlot.StartTime + blockingCount * slotDifference;
        
        var appointmentSlots = appointmentSlotRepository.GetBetween(hub, startTime, endTime, slotLength)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bay in bays)
        {
            if (bay.BayStatus == BayStatus.Closed) continue;
                

            var validBay = true;
            await foreach (var slot in appointmentSlots)
            {
                logger.LogDebug("Getting Appointment for this Bay \n({@Bay})\n in this AppointmentSlot \n({@AppointmentSlot}).", bay, slot);
            
                var appointment = await appointmentRepository.GetAsync(bay, slot, cancellationToken);
                if (appointment == null) continue;
                
                logger.LogDebug("Bay \n({@Bay})\n had a blocking appointment \n({@Appointment})\n in this AppointmentSlot \n({@AppointmentSlot}).", bay, appointment, slot);
                
                validBay = false;
                break;
            }

            if (!validBay) continue;
            
            logger.LogInformation("Bay \n({@Bay})\n does not have an Appointment assigned in this AppointmentSlot \n({@AppointmentSlot}).", bay, appointmentSlot);

            return bay;
        }

        return null;
    }

    public async Task<Tuple<AppointmentSlot, Bay>?> GetNextVacantAsync(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var appointmentSlots = appointmentSlotRepository.GetAfter(hub, startTime)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var appointmentSlot in appointmentSlots)
        {
            var bay = await GetVacantAsync(hub, appointmentSlot, cancellationToken);
            if (bay != null)
            {
                return new Tuple<AppointmentSlot, Bay>(appointmentSlot, bay);
            }
        }

        return null;
    }
    
    public async Task SetAsync(Trip trip, Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var appointmentSlotBay = await GetNextVacantAsync(hub, startTime, cancellationToken);
        if (appointmentSlotBay == null)
        {
            logger.LogError("Hub \n({@Hub})\n did not have any AppointmentSlot with available Appointments after this Step ({Step})\n for this Trip \n({@Trip}).", hub, startTime, trip);

            return;
        }

        var appointment = await GetNewObjectAsync(appointmentSlotBay.Item1, appointmentSlotBay.Item2, trip, cancellationToken);
        
        logger.LogInformation("New Appointment created: Appointment={@Appointment}", appointment);
    }
}