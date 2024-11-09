using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.BayStaffServices;

public sealed class BayStaffStepper(ModelDbContext context) : IStepperService<BayStaff>
{
    public async Task ExecuteStepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = context.BayStaffs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            await ExecuteStepAsync(bayStaff, cancellationToken);
        }
    }
}