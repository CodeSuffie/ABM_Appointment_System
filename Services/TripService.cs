using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Services;

public sealed class TripService(ModelDbContext context)
{
    public async Task InitializeObjectAsync(TruckShift truckShift, Truck truck, CancellationToken cancellationToken)
    {
        var tripType =
            ModelConfig.Random.NextDouble() > AgentConfig.PickupChance ?
                TripType.Pickup :
                TripType.Unload;
        tripType =
            ModelConfig.Random.NextDouble() > AgentConfig.DoubleTripChance ?
                TripType.UnloadPickup :
                tripType;
        
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        var destination = 
            await context.Locations.FirstOrDefaultAsync(
                x => x.Id == hub.LocationId, 
                cancellationToken);

        if (destination == null) return;
        // TODO: Maybe not return but try again with other hubs, not this one.
        
        var trip = new Trip
        {
            TripType = tripType,
            TruckShift = truckShift,
            CurrentDestination = destination,
            Truck = truck,
        };
        
        truckShift.Trips.Add(trip);
    }
    
    public async Task InitializeObjectsAsync(TruckShift truckShift, CancellationToken cancellationToken)
    {
        // TODO: Okay but all these loads are for unloading, what about pickups? I am a bit tired...
        var trucks = context.Trucks.Where(x => x.IsLoaded).ToList();
        foreach (var truck in trucks)
        {
            var trip = 
                await context.Trips.FirstOrDefaultAsync(
                    x => (x.TruckId == truck.Id),
                    cancellationToken);
            if (trip == null)
            {
                await InitializeObjectAsync(truckShift, truck, cancellationToken);
            }
        }
    }
}