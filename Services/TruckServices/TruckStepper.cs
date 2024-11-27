using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TruckServices;

public sealed class TruckStepper(
    ILogger<TruckStepper> logger,
    TruckService truckService,
    TruckRepository truckRepository,
    TripRepository tripRepository,
    ModelState modelState): IStepperService<Truck>
{
    public async Task StepAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(truck, cancellationToken);
        if (trip != null)
        {
            logger.LogDebug("Truck \n({@Truck})\n has an active Trip assigned in this Step \n({Step})",
                truck,
                modelState.ModelTime);
            
            logger.LogDebug("Truck \n({@Truck})\n will remain idle in this Step \n({Step})",
                truck,
                modelState.ModelTime);
            
            return;
        }
        
        logger.LogInformation("Truck \n({@Truck})\n has no active Trip assigned in this Step \n({Step})",
            truck,
            modelState.ModelTime);

        logger.LogDebug("Alerting Free for this Truck \n({@Truck})\n in this Step \n({Step})",
            truck,
            modelState.ModelTime);
        await truckService.AlertFreeAsync(truck, cancellationToken);
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trucks = truckRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            logger.LogDebug("Handling Step \n({Step})\n for Truck \n({@Truck})",
                modelState.ModelTime,
                truck);
            
            await StepAsync(truck, cancellationToken);
            
            logger.LogDebug("Completed handling Step \n({Step})\n for Truck \n({@Truck})",
                modelState.ModelTime,
                truck);
        }
    }
}