using Database.Models;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class AppointmentSlotStepper : IStepperService<AppointmentSlot>
{
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        // TODO: 
        return Task.CompletedTask;
    }
    
    public Task StepAsync(AppointmentSlot appointmentSlot, CancellationToken cancellationToken)
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