using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;

namespace Services;

public sealed class TruckCompanyService(
    ILogger<TruckCompanyService> logger,
    TruckCompanyRepository truckCompanyRepository,
    LocationService locationService,
    ModelState modelState) 
{
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
