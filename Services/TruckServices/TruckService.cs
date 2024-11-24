using Database.Models;
using Repositories;
using Services.ModelServices;
using Services.TruckCompanyServices;
using Settings;

namespace Services.TruckServices;

public sealed class TruckService(
    TruckCompanyService truckCompanyService,
    TruckCompanyRepository truckCompanyRepository,
    TripRepository tripRepository,
    ModelState modelState)
{
    public async Task<Truck> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Speed = modelState.AgentConfig.TruckAverageSpeed,
            Planned = false
        };

        return truck;
    }
    
    public async Task AlertClaimedAsync(Truck truck, Trip trip, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, truck, cancellationToken);
    }
    
    public async Task AlertUnclaimedAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(truck, cancellationToken);
        if (trip == null)
            throw new Exception("This Truck was just told to be unclaimed but no Trip was assigned");
        
        await tripRepository.UnsetAsync(trip, truck, cancellationToken);

        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        await truckCompanyService.AlertCompleteAsync(truckCompany, trip, cancellationToken);
    }
}