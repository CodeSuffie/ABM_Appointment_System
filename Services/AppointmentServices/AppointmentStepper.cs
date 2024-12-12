using Database.Models;
using Services.Abstractions;

namespace Services.AppointmentServices;

public sealed class AppointmentStepper : IStepperService<Appointment>
{
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public Task StepAsync(Appointment entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StepAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}