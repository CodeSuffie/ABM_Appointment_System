using Services.Abstractions;

namespace Services.Initializers;

public sealed class AppointmentInitializer : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Appointment;
    
    public Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }
}