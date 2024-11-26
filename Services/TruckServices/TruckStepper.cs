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
            logger.LogDebug("Truck ({@Truck}) has an active Trip assigned in this Step ({Step})...",
                truck,
                modelState.ModelTime);
            
            logger.LogDebug("Truck ({@Truck}) will remain idle in this Step ({Step})...",
                truck,
                modelState.ModelTime);
            
            return;
        }
        
        logger.LogInformation("Truck ({@Truck}) has no active Trip assigned in this Step ({Step}).",
            truck,
            modelState.ModelTime);

        logger.LogDebug("Alerting Free for this Truck ({@Truck}) in this Step ({Step}).",
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
            logger.LogDebug("Handling Step ({Step}) for Truck ({@Truck})...",
                modelState.ModelTime,
                truck);
            
            await StepAsync(truck, cancellationToken);
            
            logger.LogDebug("Completed handling Step ({Step}) for Truck ({@Truck}).",
                modelState.ModelTime,
                truck);
        }
    }
}