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
    TruckCompanyRepository truckCompanyRepository,
    TruckRepository truckRepository,
    TripService tripService,
    ModelState modelState)
{
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
            Speed = modelState.AgentConfig.TruckAverageSpeed,
            Planned = false
        };

        await truckRepository.AddAsync(truck, cancellationToken);

        return truck;
    }

    public async Task AlertFreeAsync(Truck truck, CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("Truck \n({@Truck})\n did not have a TruckCompany assigned to alert free for.",
                truck);

            return;
        }
        
        var trip = await tripService.GetNextAsync(truckCompany, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have a Trip for this Truck \n({@Truck})\n to start.",
                truckCompany,
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