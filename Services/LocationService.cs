using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.ModelServices;
using Services.TripServices;
using Settings;

namespace Services;

public sealed class LocationService(
    TripRepository tripRepository,
    HubRepository hubRepository,
    TruckCompanyRepository truckCompanyRepository,
    BayRepository bayRepository,
    ParkingSpotRepository parkingSpotRepository,
    ModelState modelState)
{
    public async Task InitializeObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var hubCount = await hubRepository.GetCountAsync(cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            hubCount, 
            modelState.AgentConfig.HubLocations.Length, 
            "Hub to set location for is not defined in the HubLocations Array");

        hub.XLocation = modelState.AgentConfig.HubLocations[hubCount, 0];
        hub.YLocation = modelState.AgentConfig.HubLocations[hubCount, 1];
    }
    
    public async Task InitializeObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var truckCompanyCount = await truckCompanyRepository.GetCountAsync(cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            truckCompanyCount, 
            modelState.AgentConfig.TruckCompanyLocations.Length, 
            "TruckCompany to set location for is not defined in the TruckCompanyLocations Array");

        truckCompany.XLocation = modelState.AgentConfig.HubLocations[truckCompanyCount, 0];
        truckCompany.YLocation = modelState.AgentConfig.HubLocations[truckCompanyCount, 1];
    }
    
    public async Task InitializeObjectAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
            throw new Exception("There was no Hub assigned to this Bay.");
        
        var bayCount = await bayRepository.GetCountAsync(hub, cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            bayCount, 
            modelState.AgentConfig.BayLocations.Length, 
            "Bay to set location for is not defined in the BayLocations Array");

        bay.XLocation = modelState.AgentConfig.BayLocations[bayCount, 0];
        bay.YLocation = modelState.AgentConfig.BayLocations[bayCount, 1];
    }
    
    public async Task InitializeObjectAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null)
            throw new Exception("There was no Hub assigned to this ParkingSpot.");
        
        var parkingSpotCount = await parkingSpotRepository.GetCountAsync(hub, cancellationToken);
        
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            parkingSpotCount, 
            modelState.AgentConfig.ParkingSpotLocations.Length, 
            "ParkingSpot to set location for is not defined in the ParkingSpotLocations Array");
        
        parkingSpot.XLocation = modelState.AgentConfig.ParkingSpotLocations[parkingSpotCount, 0];
        parkingSpot.YLocation = modelState.AgentConfig.ParkingSpotLocations[parkingSpotCount, 1];
    }

    public async Task SetAsync(Trip trip, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, truckCompany.XLocation, truckCompany.YLocation, cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Hub hub, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, hub.XLocation, hub.YLocation, cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, parkingSpot.XLocation, parkingSpot.YLocation, cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, bay.XLocation, bay.YLocation, cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, long xLocation, long yLocation, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, xLocation, yLocation, cancellationToken);
    }
}