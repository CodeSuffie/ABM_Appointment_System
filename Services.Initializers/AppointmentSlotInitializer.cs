using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public class AppointmentSlotInitializer(
    ILogger<AppointmentSlotInitializer> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AppointmentSlotFactory appointmentSlotFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Appointment;

    private async Task InitializeObjectsAsync(Hub hub, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        var appointmentSlotDifference = modelState.AppointmentConfig!.AppointmentSlotDifference * modelState.ModelConfig.ModelStep;
        var appointmentSlotInitialDelay = modelState.AppointmentConfig!.AppointmentSlotInitialDelay * modelState.ModelConfig.ModelStep;
        var appointmentSlotCount = (int)(operatingHour.Duration / appointmentSlotDifference);
        if (appointmentSlotCount <= 0) return;

        for (var i = 0; i < appointmentSlotCount - 1; i++)
        {
            await appointmentSlotFactory.GetNewObjectAsync(
                hub,
                (appointmentSlotInitialDelay + i * appointmentSlotDifference + operatingHour.StartTime),
                cancellationToken);
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