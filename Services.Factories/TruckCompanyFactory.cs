using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class TruckCompanyFactory(
    ILogger<TruckCompanyFactory> logger,
    TruckCompanyRepository truckCompanyRepository,
    LocationService locationService,
    ModelState modelState) : IFactoryService<TruckCompany>
{
    public async Task<TruckCompany?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany();

        await truckCompanyRepository.AddAsync(truckCompany, cancellationToken);
        
        logger.LogDebug("Setting location for this TruckCompany \n({@TruckCompany})",
            truckCompany);
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);

        return truckCompany;
    }
}