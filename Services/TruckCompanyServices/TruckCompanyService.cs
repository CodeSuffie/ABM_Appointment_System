using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyService(
    ILogger<TruckCompanyService> logger,
    TruckCompanyRepository truckCompanyRepository,
    LocationService locationService,
    ModelState modelState) 
{
    public async Task<TruckCompany> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany
        {
            XSize = 1,
            YSize = 1
        };

        await truckCompanyRepository.AddAsync(truckCompany, cancellationToken);
        
        logger.LogDebug("Setting location for this TruckCompany \n({@TruckCompany})",
            truckCompany);
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);
        
        // Initially no Trips are created

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
