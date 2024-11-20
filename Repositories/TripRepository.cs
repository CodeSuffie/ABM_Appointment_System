using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TripRepository(
    ModelDbContext context,
    WorkRepository workRepository,
    BayRepository bayRepository,
    ParkingSpotRepository parkingSpotRepository)
{
    public async Task<IQueryable<Trip>> GetTripsByHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .Where(x => x.HubId == hub.Id);

        return trips;
    }
    
    public async Task<IQueryable<Trip>> GetCurrentTripsByHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var trips = (await GetTripsByHubAsync(hub, cancellationToken))
            .Where(x => x.Work != null && 
                        x.Work.WorkType != WorkType.TravelHub);

        return trips;
    }
    
    public async Task<IQueryable<Trip>> GetCurrentTripsByHubByWorkTypeAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await GetCurrentTripsByHubAsync(hub, cancellationToken))
            .Where(x => x.Work != null &&
                        x.Work.WorkType == workType);
        
        return trips;
    }
    
    public async Task<Trip?> GetNextTripByHubByWorkTypeAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await GetCurrentTripsByHubByWorkTypeAsync(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        await foreach (var trip in trips)
        {
            var work = await workRepository.GetWorkByTripAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            nextTrip = trip;
            earliestStart = work?.StartTime;
        }

        return nextTrip;
    }
    
    public async Task<Trip?> GetTripByParkingSpotAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(x => x.Id == parkingSpot.TripId, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetTripByBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t=> t.Bay != null && t.BayId == bay.Id, cancellationToken);
        
        return trip;
    }
    
    public async Task<Trip?> GetTripByWorkAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.TripId == null) return null;
        
        var trip = await context.Trips
            .FirstOrDefaultAsync(x => x.Id == work.TripId, cancellationToken);

        return trip;
    }
    
    public async Task RemoveTripBayAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Trip.Bay = null;
        // TODO: Bay.Trip = null;
        // TODO: Save
    }

    public async Task SetTripBayAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var oldBay = await bayRepository.GetBayByTripAsync(trip, cancellationToken);
        if (oldBay != null)
            throw new Exception("Trip already has an assigned Bay, it cannot move to another.");
        
        throw new NotImplementedException();
        // TODO: trip.Bay = bay;
        // TODO: bay.Trip = trip;
        // TODO: Save
    }
    
    public async Task RemoveTripParkingSpotAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Trip.Bay = null;
        // TODO: bay.Trip = null;
        // TODO: Save
    }

    public async Task SetTripParkingSpotAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldParkingSpot = await parkingSpotRepository.GetParkingSpotByTripAsync(trip, cancellationToken);
        if (oldParkingSpot != null)
            throw new Exception("Trip already has an assigned ParkingSpot, it cannot move to another.");
        
        throw new NotImplementedException();
        // TODO: trip.ParkingSpot = parkingSpot;
        // TODO: parkingSpot.Trip = trip;
        // TODO: Save
    }
}