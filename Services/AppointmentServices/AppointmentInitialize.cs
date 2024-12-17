using Services.Abstractions;

namespace Services.AppointmentServices;

public sealed class AppointmentInitialize : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Appointment;
    
    public Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }
}