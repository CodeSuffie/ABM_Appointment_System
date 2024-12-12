using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;
using Services.TripServices;
using Services.TruckCompanyServices;

namespace Services.TruckServices;

public sealed class TruckService(
    ILogger<TruckService> logger,
    TruckCompanyService truckCompanyService,
    TruckRepository truckRepository,
    TripService tripService,
    ModelState modelState)
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.TruckSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.TruckAverageSpeed + deviation;
    }
    
    public async Task<Truck?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("No TruckCompany could be selected for the new Truck");

            return null;
        }
        
        logger.LogDebug("TruckCompany \n({@TruckCompany})\n was selected for the new Truck.",
            truckCompany);
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Speed = GetSpeed(),
            Capacity = modelState.AgentConfig.TruckAverageCapacity
        };

        await truckRepository.AddAsync(truck, cancellationToken);

        return truck;
    }

    public TimeSpan GetTravelTime(Truck truck, TruckCompany truckCompany, Hub hub)
    {
        var xDiff = Math.Abs(truckCompany.XLocation - hub.XLocation);
        var xSteps = (int) Math.Ceiling((double) xDiff / (double) truck.Speed);
        
        var yDiff = Math.Abs(truckCompany.YLocation - hub.YLocation);
        var ySteps = (int) Math.Ceiling((double) yDiff / (double) truck.Speed);

        return xSteps >= ySteps ? 
            xSteps * modelState.ModelConfig.ModelStep : 
            ySteps * modelState.ModelConfig.ModelStep;
    }

    public async Task AlertFreeAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await tripService.GetNextAsync(truck, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Truck \n({@Truck})\n could not receive a Trip to start.",
                truck);
            
            logger.LogDebug("Truck \n({@Truck})\n will remain idle...",
                truck);
            
            return;
        }
        
        logger.LogDebug("Alerting Free for this Truck \n({@Truck})\n to selected Trip \n({@Trip})",
            truck,
            trip);
        await tripService.AlertFreeAsync(trip, truck, cancellationToken);
    }
}