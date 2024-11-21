using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyStepper(TruckCompanyRepository truckCompanyRepository) : IStepperService<TruckCompany>
{
    public async Task StepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create new Trips [tripService.GetNewObjectAsync(truckCompany, cancellationToken)]
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = (await truckCompanyRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            await StepAsync(truckCompany, cancellationToken);
        }
    }
}