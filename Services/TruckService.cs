using Database.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;

namespace Services;

public sealed class TruckService(
    ILogger<TruckService> logger,
    TripService tripService,
    ModelState modelState)
{
    public async Task AlertFreeAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await tripService.GetNextAsync(truck, cancellationToken);
        if (trip == null)
        {
            logger.LogInformation("Truck \n({@Truck})\n could not receive a Trip to start.", truck);
            
            logger.LogDebug("Truck \n({@Truck})\n will remain idle...", truck);
            
            return;
        }
        
        logger.LogDebug("Alerting Free for this Truck \n({@Truck})\n to selected Trip \n({@Trip})", truck, trip);
        await tripService.AlertFreeAsync(trip, truck, cancellationToken);
    }
}