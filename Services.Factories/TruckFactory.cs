using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class TruckFactory(
    ILogger<TruckFactory> logger,
    TruckRepository truckRepository,
    TruckCompanyFactory truckCompanyFactory,
    ModelState modelState) : IFactoryService<Truck>
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.TruckSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.TruckAverageSpeed + deviation;
    }
    
    public async Task<Truck?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyFactory.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("No TruckCompany could be selected for the new Truck");

            return null;
        }
        
        logger.LogDebug("TruckCompany \n({@TruckCompany})\n was selected for the new Truck.", truckCompany);
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Speed = GetSpeed(),
            Capacity = modelState.AgentConfig.TruckAverageCapacity
        };

        await truckRepository.AddAsync(truck, cancellationToken);

        return truck;
    }
}