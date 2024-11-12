using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.TripServices;
using Settings;

namespace Services.HubServices;

public sealed class HubService(
    ModelDbContext context,
    TripService tripService)
{
    public async Task<Hub> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = new Hub
        {
            XSize = AgentConfig.HubXSize,
            YSize = AgentConfig.HubYSize,
            OperatingChance = AgentConfig.HubAverageOperatingDays,
            AverageOperatingHourLength = AgentConfig.OperatingHourAverageLength
        };

        return hub;
    }

    public async Task<Hub> SelectHubAsync(CancellationToken cancellationToken)
    {
        var hubs = await context.Hubs
                .ToListAsync(cancellationToken);
            
        if (hubs.Count <= 0) throw new Exception("There was no Hub to select.");
            
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        return hub;
    }

    public async Task<IQueryable<Trip>> GetTripsAtHubAsync(Hub hub, CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .Where(x => x.HubId == hub.Id)
            .Where(x => x.Work != null && 
                        x.Work.WorkType != WorkType.Travel);

        return trips;
    }

    public async Task<Trip?> GetNextCheckInTripAsync(Hub hub, CancellationToken cancellationToken)
    {
        var allTrips = await GetTripsAtHubAsync(hub, cancellationToken);
        
        var trips = allTrips
            .Where(x => x.Work != null &&
                        x.Work.WorkType == WorkType.WaitCheckIn)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        
        await foreach (var trip in trips)
        {
            var work = await tripService.GetWorkForTripAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            nextTrip = trip;
            earliestStart = work?.StartTime;
        }

        return nextTrip;
    }
    
    public async Task<Trip?> GetNextBayTripAsync(Hub hub, CancellationToken cancellationToken)
    {
        var allTrips = await GetTripsAtHubAsync(hub, cancellationToken);
        
        var trips = allTrips
            .Where(x => x.Work != null &&
                        x.Work.WorkType == WorkType.WaitBay)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        
        await foreach (var trip in trips)
        {
            var work = await tripService.GetWorkForTripAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            nextTrip = trip;
            earliestStart = work?.StartTime;
        }

        return nextTrip;
    }
}
