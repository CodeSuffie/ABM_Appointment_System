using Database.Models;
using Services.Abstractions;

namespace Services.AppointmentSlotServices;

public sealed class AppointmentSlotStepper : IStepperService<AppointmentSlot>
{
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public Task StepAsync(AppointmentSlot entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}