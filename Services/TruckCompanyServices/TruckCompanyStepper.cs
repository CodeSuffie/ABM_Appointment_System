using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Services.TripServices;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyStepper(
    ILogger<TruckCompanyStepper> logger,
    TruckCompanyRepository truckCompanyRepository,
    TripService tripService,
    ModelState modelState) : IStepperService<TruckCompany>
{
    public async Task StepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        // logger.LogDebug("Adding new Trips for TruckCompany \n({@TruckCompany})",
            // truckCompany);
        
        // await tripService.AddNewObjectsAsync(truckCompany, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = truckCompanyRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            logger.LogDebug("Handling Step \n({Step})\n for TruckCompany \n({@TruckCompany})",
                modelState.ModelTime,
                truckCompany);
            
            await StepAsync(truckCompany, cancellationToken);
            
            logger.LogDebug("Completed handling Step \n({Step})\n for TruckCompany \n({@TruckCompany})",
                modelState.ModelTime,
                truckCompany);
        }
    }
}