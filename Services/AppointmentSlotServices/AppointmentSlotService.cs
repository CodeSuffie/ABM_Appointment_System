using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.AppointmentSlotServices;

public sealed class AppointmentSlotService(
    ILogger<AppointmentSlotService> logger,
    AppointmentRepository appointmentRepository,
    AppointmentSlotRepository appointmentSlotRepository,
    BayRepository bayRepository,
    ModelState modelState)
{
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