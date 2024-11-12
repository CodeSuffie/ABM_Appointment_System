using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyStepper(ModelDbContext context) : IStepperService<TruckCompany>
{
    public async Task StepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create new Trips [tripService.GetNewObjectAsync(truckCompany, cancellationToken)]
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            await StepAsync(truckCompany, cancellationToken);
        }
    }
}