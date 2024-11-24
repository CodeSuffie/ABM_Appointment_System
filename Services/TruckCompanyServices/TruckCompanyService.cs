using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.ModelServices;
using Services.TripServices;
using Settings;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyService(
    ModelState modelState,
    TripLogger tripLogger,
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
    
    public async Task<TruckCompany> SelectTruckCompanyAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = await (await truckCompanyRepository.GetAsync(cancellationToken))
            .ToListAsync(cancellationToken);
        
        if (truckCompanies.Count <= 0) 
            throw new Exception("There was no Truck Company to assign this new Truck to.");
            
        var truckCompany = truckCompanies[modelState.Random(truckCompanies.Count)];
        return truckCompany;
    }

    public async Task AlertCompleteAsync(TruckCompany truckCompany, Trip trip, CancellationToken cancellationToken)
    {
        await tripLogger.LogAsync(trip, LogType.Success, "Completed.", cancellationToken);
        // TODO: Log Complete for Model [modelLogger.Log(trip, LogType.Success, "Completed.")]
    }
}
