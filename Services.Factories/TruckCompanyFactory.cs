using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class TruckCompanyFactory(
    ILogger<TruckCompanyFactory> logger,
    TruckCompanyRepository truckCompanyRepository,
    LocationFactory locationFactory,
    ModelState modelState) : IFactoryService<TruckCompany>
{
    public async Task<TruckCompany?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany();

        await truckCompanyRepository.AddAsync(truckCompany, cancellationToken);
        
        logger.LogDebug("Setting location for this TruckCompany \n({@TruckCompany})",
            truckCompany);
        await locationFactory.InitializeObjectAsync(truckCompany, cancellationToken);

        return truckCompany;
    }
    
    public async Task<TruckCompany?> SelectTruckCompanyAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = await (truckCompanyRepository.Get())
            .ToListAsync(cancellationToken);
        
        if (truckCompanies.Count <= 0) 
        {
            logger.LogError("Model did not have a Truck Company assigned.");

            return null;
        }
            
        var truckCompany = truckCompanies[modelState.Random(truckCompanies.Count)];
        return truckCompany;
    }
}