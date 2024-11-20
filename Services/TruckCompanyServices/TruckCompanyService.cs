using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Settings;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyService(
    TruckCompanyRepository truckCompanyRepository) 
{
    public async Task<TruckCompany> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany
        {
            XSize = 1,
            YSize = 1
        };

        return truckCompany;
    }
    
    // TODO: Repository
    public async Task<TruckCompany> SelectTruckCompanyAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = await (await truckCompanyRepository.GetTruckCompaniesAsync(cancellationToken))
            .ToListAsync(cancellationToken);
        
        if (truckCompanies.Count <= 0) 
            throw new Exception("There was no Truck Company to assign this new Truck to.");
            
        var truckCompany = truckCompanies[ModelConfig.Random.Next(truckCompanies.Count)];
        return truckCompany;
    }
}
