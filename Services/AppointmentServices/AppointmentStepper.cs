using Database.Models;
using Services.Abstractions;

namespace Services.AppointmentServices;

public sealed class AppointmentStepper : IStepperService<Appointment>
{
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }
    
    public Task StepAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }
}