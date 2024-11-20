using Database.Models;
using Services.TruckCompanyServices;
using Settings;

namespace Services.TruckServices;

public sealed class TruckService(TruckCompanyService truckCompanyService)
{
    public async Task<Truck> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Capacity = AgentConfig.TruckAverageCapacity,
            Planned = false
        };

        return truck;
    }
}