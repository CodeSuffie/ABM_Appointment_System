using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class TripFactory(
    ILogger<TripFactory> logger,
    TripRepository tripRepository,
    TruckCompanyRepository truckCompanyRepository,
    LocationFactory locationFactory,
    AppointmentRepository appointmentRepository,
    AppointmentFactory appointmentFactory,
    ModelState modelState) : IFactoryService<Trip>
{
    private TimeSpan GetTravelTime(Truck truck, TruckCompany truckCompany, Hub hub)
    {
        var xDiff = Math.Abs(truckCompany.XLocation - hub.XLocation);
        var xSteps = (int) Math.Ceiling((double) xDiff / (double) truck.Speed);
        
        var yDiff = Math.Abs(truckCompany.YLocation - hub.YLocation);
        var ySteps = (int) Math.Ceiling((double) yDiff / (double) truck.Speed);

        return xSteps >= ySteps ? 
            xSteps * modelState.ModelConfig.ModelStep : 
            ySteps * modelState.ModelConfig.ModelStep;
    }
    
    public async Task<Trip?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var trip = new Trip
        {
            Completed = false
        };
        
        await tripRepository.AddAsync(trip, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetNewObjectAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await GetNewObjectAsync(cancellationToken);
        if (trip == null)
        {
            logger.LogError("Trip could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting Truck \n({@Truck})\n to this Trip \n({@Trip}).", truck, trip);
        await tripRepository.SetAsync(trip, truck, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetNewObjectAsync(Truck truck, Hub hub, CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) to create the new Trip for.", truck);

            return null;
        }
        
        var trip = await GetNewObjectAsync(truck, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Trip could not be created for this Truck \n({@Truck}).", truck);

            return null;
        }
        
        logger.LogDebug("Setting Hub \n({@Hub})\n to this Trip \n({@Trip}).", hub, trip);
        await tripRepository.SetAsync(trip, hub, cancellationToken);
                
        logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n location to this Trip \n({@Trip})", truckCompany, trip);
        await locationFactory.SetAsync(trip, truckCompany, cancellationToken);

        if (!modelState.ModelConfig.AppointmentSystemMode) return trip;
        
        logger.LogDebug("Getting Travel Time for this Truck \n({@Truck})\n from this TruckCompany \n({@TruckCompany})\n location to this Hub \n({@Hub})\n location.", truck, truckCompany, hub);
        var travelTime = GetTravelTime(truck, truckCompany, hub);
            
        logger.LogDebug("Setting Travel Time ({Step}) for to this Trip \n({@Trip}).", travelTime, trip);
        await tripRepository.SetAsync(trip, travelTime, cancellationToken);

        var earliestArrivalTime = modelState.ModelTime + travelTime;
            
        logger.LogDebug("Setting Appointment at this Hub \n({@Hub})\n for Trip \n({@Trip})\n with the calculated earliest arrival time ({Step}).", hub, trip, earliestArrivalTime);
        await appointmentFactory.SetAsync(trip, hub, earliestArrivalTime, cancellationToken);

        logger.LogDebug("Getting created Appointment for this Trip \n({@Trip}).", trip);
        var appointment = await appointmentRepository.GetAsync(trip, cancellationToken);
        if (appointment != null) return trip;
            
        logger.LogError("No Appointment was assigned to the Trip to create.");
        return null;
    }
}