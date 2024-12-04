using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repositories;

public sealed class TripRepository(
    ILogger<TripRepository> logger,
    ModelDbContext context,
    BayRepository bayRepository,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository,
    TruckRepository truckRepository,
    HubRepository hubRepository,
    LoadRepository loadRepository)
{
    public IQueryable<Trip> Get()
    {
        var trips = context.Trips;

        return trips;
    }

    public IQueryable<Trip> Get(bool active)
    {
        var trips = context.Trips
            .Where(t => (t.Truck != null) == active);

        return trips;
    }
    
    public IQueryable<Trip> Get(Hub hub)
    {
        return Get().Where(t => t.HubId == hub.Id);
    }
    
    public IQueryable<Trip> Get(TruckCompany truckCompany)
    {
        return Get().Where(t => t.DropOff != null ||
                                t.PickUp != null)
            .Where(t => t.DropOff == null ||
                        t.DropOff.TruckCompanyStartId == truckCompany.Id)
            .Where(t => t.PickUp == null ||
                        t.PickUp.TruckCompanyEndId == truckCompany.Id);
    }
    
    public Task<Trip?> GetAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(t => t.Id == parkingSpot.TripId, cancellationToken);
    }
    
    public Task<Trip?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        return context.Trips
            .FirstOrDefaultAsync(t => t.Id == adminStaff.TripId, cancellationToken);
    }
    
    public Task<Trip?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        return context.Trips
            .FirstOrDefaultAsync(t=> t.Id == bay.TripId, cancellationToken);
    }
    
    public Task<Trip?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        return context.Trips
            .FirstOrDefaultAsync(t => t.Id == work.TripId, cancellationToken);
    }
    
    public Task<Trip?> GetAsync(Truck truck, CancellationToken cancellationToken)
    {
        return context.Trips
            .FirstOrDefaultAsync(t=> t.Id == truck.TripId, cancellationToken);
    }
    
    public IQueryable<Trip> GetCurrent(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = Get(hub)
            .Where(t => t.Work != null &&
                        t.Work.WorkType == workType);
        
        return trips;
    }
    
    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        await context.Trips
            .AddAsync(trip, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldParkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (oldParkingSpot != null)
        {
            logger.LogError("Trip ({@Trip}) already has an assigned ParkingSpot ({@ParkingSpot}), it cannot move to the new ParkingSpot ({@ParkingSpot}).",
                trip,
                oldParkingSpot,
                parkingSpot);

            return;
        }
        
        trip.ParkingSpot = parkingSpot;
        parkingSpot.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var oldAdminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
        if (oldAdminStaff != null)
        {
            logger.LogError("Trip ({@Trip}) already has an assigned AdminStaff ({@AdminStaff}), it cannot move to the new AdminStaff ({@AdminStaff}).",
                trip,
                oldAdminStaff,
                adminStaff);

            return;
        }
        
        trip.AdminStaff = adminStaff;
        adminStaff.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var oldBay = await bayRepository.GetAsync(trip, cancellationToken);
        if (oldBay != null)
        {
            logger.LogError("Trip ({@Trip}) already has an assigned Bay ({@Bay}), it cannot move to the new Bay ({@Bay}).",
                trip,
                oldBay,
                bay);

            return;
        }
        
        trip.Bay = bay;
        bay.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var oldTruck = await truckRepository.GetAsync(trip, cancellationToken);
        if (oldTruck != null)
        {
            logger.LogError("Trip ({@Trip}) already has an assigned Truck ({@Truck}), it cannot move to the new Truck ({@Truck}).",
                trip,
                oldTruck,
                truck);

            return;
        }
        
        trip.Truck = truck;
        truck.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public Task SetAsync(Trip trip, long xLocation, long yLocation, CancellationToken cancellationToken)
    {
        trip.XLocation = xLocation;
        trip.YLocation = yLocation;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    
    public async Task SetAsync(Trip trip, bool completed, CancellationToken cancellationToken)
    {
        trip.Completed = completed;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    
    public async Task SetDropOffAsync(Trip trip, Load dropOff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(dropOff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Load ({@Load}) to set as Drop-Off for this Trip ({@Trip}) has no Hub assigned.",
                dropOff,
                trip);
            
            logger.LogDebug("Removing invalid Drop-Off Load ({@Load}) for this Trip ({@Trip}).",
                dropOff,
                trip);
            await loadRepository.RemoveAsync(dropOff, cancellationToken);
            
            return;
        }

        if (trip.Hub == null)
        {
            trip.Hub = hub;
            hub.Trips.Add(trip);
        }
        else if (trip.HubId != hub.Id)
        {
            logger.LogError(
                "Load ({@Load}) to set as Drop-Off for this Trip ({@Trip}) has a different Hub ({@Hub}) " +
                "assigned than the Hub Pick-Up Load has assigned ({@Hub}).",
                dropOff,
                trip,
                hub,
                trip.Hub);

            return;
        }
        
        trip.DropOff = dropOff;
        dropOff.DropOffTrip = trip;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPickUpAsync(Trip trip, Load pickUp, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(pickUp, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Load ({@Load}) to set as Pick-Up for this Trip ({@Trip}) has no Hub assigned.",
                pickUp,
                trip);
            
            logger.LogDebug("Removing invalid Pick-Up Load ({@Load}) for this Trip ({@Trip}).",
                pickUp,
                trip);
            await loadRepository.RemoveAsync(pickUp, cancellationToken);
            
            return;
        }

        if (trip.Hub == null)
        {
            trip.Hub = hub;
            hub.Trips.Add(trip);
        }
        else if (trip.HubId != hub.Id)
        {
            logger.LogError(
                "Load ({@Load}) to set as Pick-Up for this Trip ({@Trip}) has a different Hub ({@Hub}) " +
                "assigned than the Hub Drop-Off Load has assigned ({@Hub}).",
                pickUp,
                trip,
                hub,
                trip.Hub);

            return;
        }
        
        trip.PickUp = pickUp;
        pickUp.PickUpTrip = trip;

        await context.SaveChangesAsync(cancellationToken);
    }


    public Task UnsetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        trip.ParkingSpot = null;
        parkingSpot.Trip = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task UnsetAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        trip.AdminStaff = null;
        adminStaff.Trip = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task UnsetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        trip.Bay = null;
        bay.Trip = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }
    
    public Task UnsetAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        trip.Truck = null;
        truck.Trip = null;
        
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(bool claimed, CancellationToken cancellationToken)
    {
        return Get(claimed)
            .Where(t => !t.Completed)
            .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(WorkType workType, CancellationToken cancellationToken)
    {
        return Get()
            .Where(t => t.Work != null &&
                        t.Work.WorkType == workType)
            .CountAsync(cancellationToken);
    }

    public Task<int> CountCompletedAsync(CancellationToken cancellationToken)
    {
        return Get()
            .Where(t => t.Completed)
            .CountAsync(cancellationToken);
    }
}