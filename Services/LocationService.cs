using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class LocationService(
    ILogger<LocationService> logger,
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

        if (hubCount > modelState.AgentConfig.HubLocations.GetLength(0))
        {
            logger.LogError("Hub \n({@Hub})\n could not have its location initialized because " +
                            "its location is not defined in the agent configuration.",
                hub);
            
            return;
        }

        hub.XLocation = modelState.AgentConfig.HubLocations[hubCount - 1, 0];
        hub.YLocation = modelState.AgentConfig.HubLocations[hubCount - 1, 1];
        
        logger.LogInformation("Location successfully initialized for this Hub \n({@Hub})",
            hub);
    }
    
    public async Task InitializeObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var truckCompanyCount = await truckCompanyRepository.GetCountAsync(cancellationToken);
        
        if (truckCompanyCount > modelState.AgentConfig.TruckCompanyLocations.GetLength(0))
        {
            logger.LogError("TruckCompany \n({@TruckCompany})\n could not have its location initialized because " +
                            "its location is not defined in the agent configuration.",
                truckCompany);
            
            return;
        }

        truckCompany.XLocation = modelState.AgentConfig.TruckCompanyLocations[truckCompanyCount - 1, 0];
        truckCompany.YLocation = modelState.AgentConfig.TruckCompanyLocations[truckCompanyCount - 1, 1];
        
        logger.LogInformation("Location successfully initialized for this TruckCompany \n({@TruckCompany})",
            truckCompany);
    }
    
    public async Task InitializeObjectAsync(Bay bay, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Bay \n({@Bay})\n did not have a Hub assigned to initialize its location with.",
                bay);

            return;
        }
        
        var bayCount = await bayRepository.GetCountAsync(hub, cancellationToken);
        
        if (bayCount > modelState.AgentConfig.BayLocations.GetLength(0))
        {
            logger.LogError("Bay \n({@Bay})\n could not have its location initialized because " +
                            "its location is not defined in the agent configuration.",
                bay);
            
            return;
        }

        bay.XLocation = hub.XLocation + modelState.AgentConfig.BayLocations[bayCount - 1, 0];
        bay.YLocation = hub.YLocation + modelState.AgentConfig.BayLocations[bayCount - 1, 1];
    }
    
    public async Task InitializeObjectAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(parkingSpot, cancellationToken);
        if (hub == null)
        {
            logger.LogError("ParkingSpot \n({@ParkingSpot})\n did not have a Hub assigned to initialize its location with.",
                parkingSpot);

            return;
        }
        
        var parkingSpotCount = await parkingSpotRepository.GetCountAsync(hub, cancellationToken);
        
        if (parkingSpotCount > modelState.AgentConfig.ParkingSpotLocations.GetLength(0))
        {
            logger.LogError("ParkingSpot \n({@ParkingSpot})\n could not have its location initialized because " +
                            "its location is not defined in the agent configuration.",
                parkingSpot);
            
            return;
        }
        
        parkingSpot.XLocation = hub.XLocation + modelState.AgentConfig.ParkingSpotLocations[parkingSpotCount - 1, 0];
        parkingSpot.YLocation = hub.YLocation + modelState.AgentConfig.ParkingSpotLocations[parkingSpotCount - 1, 1];
    }

    public Task SetAsync(Trip trip, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        return tripRepository.SetAsync(trip, truckCompany.XLocation, truckCompany.YLocation, cancellationToken);
    }
    
    public Task SetAsync(Trip trip, Hub hub, CancellationToken cancellationToken)
    {
        return tripRepository.SetAsync(trip, hub.XLocation, hub.YLocation, cancellationToken);
    }
    
    public Task SetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        return tripRepository.SetAsync(trip, parkingSpot.XLocation, parkingSpot.YLocation, cancellationToken);
    }
    
    public Task SetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        return tripRepository.SetAsync(trip, bay.XLocation, bay.YLocation, cancellationToken);
    }
    
    public Task SetAsync(Trip trip, long xLocation, long yLocation, CancellationToken cancellationToken)
    {
        return tripRepository.SetAsync(trip, xLocation, yLocation, cancellationToken);
    }
}