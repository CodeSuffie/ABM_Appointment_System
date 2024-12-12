using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.AppointmentSlotServices;

public class AppointmentSlotInitialize(
    ILogger<AppointmentSlotInitialize> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AppointmentSlotRepository appointmentSlotRepository,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Appointment;

    private async Task InitializeObjectsAsync(Hub hub, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var appointmentSlotLength = modelState.AppointmentConfig!.AppointmentLength * modelState.ModelConfig.ModelStep;
        var appointmentSlotCount = (int)(operatingHour.Duration / appointmentSlotLength);
        if (appointmentSlotCount <= 0) return;

        var appointmentSlots = Enumerable.Range(0, appointmentSlotCount - 1)
            .Select(x => new AppointmentSlot
            {
                StartTime = x * appointmentSlotLength + operatingHour.StartTime,
                Hub = hub
            })
            .ToList();

        foreach (var appointmentSlot in appointmentSlots)
        {
            await appointmentSlotRepository.AddAsync(appointmentSlot, cancellationToken);
            await appointmentSlotRepository.SetAsync(appointmentSlot, hub, cancellationToken);
        }
    }
    
    private async Task InitializeObjectsAsync(Hub hub, CancellationToken cancellationToken)
    {
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            await InitializeObjectsAsync(hub, operatingHour, cancellationToken);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            await InitializeObjectsAsync(hub, cancellationToken);
        }
    }
}