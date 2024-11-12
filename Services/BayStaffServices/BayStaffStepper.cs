using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.BayStaffServices;

public sealed class BayStaffStepper(ModelDbContext context) : IStepperService<BayStaff>
{
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: If no active Shift, wait
        // TODO: If start of Shift, and first one at my Bay, alert my Bay it is opened
        // TODO: If end of Shift, and last one at my Bay, alert my Bay it is closed
        // TODO: If Truck at my Bay, If PickUp Load is not available at this Bay, and no one else is fetching it, fetch the Load
        // TODO: If Truck at my Bay, continue handling their Trip
        // TODO: If part of their Trip is completed alert Bay
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = context.BayStaffs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            await StepAsync(bayStaff, cancellationToken);
        }
    }
}