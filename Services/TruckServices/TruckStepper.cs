using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;
using Services.TripServices;

namespace Services.TruckServices;

public sealed class TruckStepper(
    TruckRepository truckRepository,
    TripRepository tripRepository,
    TripService tripService,
    TruckCompanyRepository truckCompanyRepository): IStepperService<Truck>
{
    public async Task StepAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(truck, cancellationToken);
        if (trip == null)
        {
            var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
            var newTrip = await tripService.SelectTripAsync(truckCompany, cancellationToken);
            
            await tripService.AlertFreeAsync(newTrip, truck, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var trucks = (await truckRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            await StepAsync(truck, cancellationToken);
        }
    }
}