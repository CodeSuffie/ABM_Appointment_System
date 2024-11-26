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
        
        logger.LogDebug("TruckCompany ({@TruckCompany}) was selected for the new Truck.",
            truckCompany);
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Speed = modelState.AgentConfig.TruckAverageSpeed,
            Planned = false
        };

        return truck;
    }

    public async Task AlertFreeAsync(Truck truck, CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("Truck ({@Truck}) did not have a TruckCompany assigned to alert free for.",
                truck);

            return;
        }
        
        var trip = await tripService.GetNextAsync(truckCompany, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("TruckCompany ({@TruckCompany}) did not have a Trip for this Truck ({@Truck}) to start.",
                truckCompany,
                truck);
            
            logger.LogDebug("Truck ({@Truck}) will remain idle...",
                truck);
            
            return;
        }
        
        logger.LogDebug("Alerting Free for this Truck ({@Truck}) to selected Trip ({@Trip})...",
            truck,
            trip);
        await tripService.AlertFreeAsync(trip, truck, cancellationToken);
    }
}