using Database;
using Database.Models;
using Services.Abstractions;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyStepper(ModelDbContext context) : IStepperService<TruckCompany>
{
    public async Task ExecuteStepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            await ExecuteStepAsync(truckCompany, cancellationToken);
        }
    }
}