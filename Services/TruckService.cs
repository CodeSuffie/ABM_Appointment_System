using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class TruckService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        truckCompany.Trucks.Add(new Truck
        {
            TruckCompany = truckCompany,
            Capacity = AgentConfig.TruckAverageCapacity
        });
    }
    
    public async Task InitializeObjectsAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCountPerTruckCompany; i++)
        {
            await InitializeObjectAsync(truckCompany, cancellationToken);
        }
    }
}