using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;
using Services.TripServices;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyStepper(
    TruckCompanyRepository truckCompanyRepository,
    TripService tripService) : IStepperService<TruckCompany>
{
    public async Task StepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await tripService.AddNewObjectsAsync(truckCompany, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = truckCompanyRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            await StepAsync(truckCompany, cancellationToken);
        }
    }
}