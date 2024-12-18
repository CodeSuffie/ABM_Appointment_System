using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class AppointmentSlotFactory(
    ILogger<AppointmentSlotFactory> logger,
    AppointmentSlotRepository appointmentSlotRepository,
    AppointmentRepository appointmentRepository,
    BayRepository bayRepository,
    ModelState modelState) : IFactoryService<AppointmentSlot>
{
    public async Task<AppointmentSlot?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var appointmentSlot = new AppointmentSlot();
        
        await appointmentSlotRepository.AddAsync(appointmentSlot, cancellationToken);

        return appointmentSlot;
    }
    
    public async Task<AppointmentSlot?> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var appointmentSlot = await GetNewObjectAsync(cancellationToken);
        if (appointmentSlot == null)
        {
            logger.LogError("AppointmentSlot could not be created for this Hub \n({@Hub}).",
                hub);

            return null;
        }
        
        await appointmentSlotRepository.SetAsync(appointmentSlot, hub, cancellationToken);

        return appointmentSlot;
    }
    
    public async Task<AppointmentSlot?> GetNewObjectAsync(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var appointmentSlot = await GetNewObjectAsync(hub, cancellationToken);
        if (appointmentSlot == null)
        {
            logger.LogError("AppointmentSlot could not be created for this Hub \n({@Hub})\n with this StartTime ({Step}).",
                hub,
                startTime);

            return null;
        }
        
        await appointmentSlotRepository.SetAsync(appointmentSlot, startTime, cancellationToken);

        return appointmentSlot;
    }
    
    public async Task<AppointmentSlot?> GetNextVacantAsync(Hub hub, TimeSpan startTime, CancellationToken cancellationToken)
    {
        var bayCount = await bayRepository.CountAsync(hub, cancellationToken);
        
        var appointmentSlots = appointmentSlotRepository.GetAfter(hub, startTime)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var appointmentSlot in appointmentSlots)
        {
            var appointmentCount = await appointmentRepository.CountAsync(appointmentSlot, cancellationToken);

            if (appointmentCount < bayCount)
            {
                logger.LogInformation("Hub \n({@Hub})\n with this AppointmentSlot \n({@AppointmentSlot})\n has more " +
                                      "Bays ({@Count}) than taken Appointments ({@Count}) and can therefore add an " +
                                      "Appointment in this Slot.",
                    hub,
                    appointmentSlot,
                    bayCount,
                    appointmentCount);
                
                return appointmentSlot;
            }
        }

        return null;
    }
}