using Database;
using Database.Models;
using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TripRepository(
    ModelDbContext context,
    BayRepository bayRepository,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository,
    TruckRepository truckRepository)
{
    public Task<IQueryable<Trip>> GetAsync(CancellationToken cancellationToken)
    {
        var trips = context.Trips;

        return Task.FromResult<IQueryable<Trip>>(trips);
    }
    
    public Task<IQueryable<Trip>> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .Where(t => t.HubId == hub.Id);

        return Task.FromResult(trips);
    }
    
    public Task<IQueryable<Trip>> GetAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .Where(t => t.DropOff != null ||
                        t.PickUp != null)
            .Where(t => t.DropOff == null ||
                        t.DropOff.TruckCompanyStartId == truckCompany.Id)
            .Where(t => t.PickUp == null ||
                        t.PickUp.TruckCompanyEndId == truckCompany.Id);
        
        return Task.FromResult(trips);
    }
    
    public async Task<Trip?> GetAsync(ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t => t.ParkingSpotId == parkingSpot.Id, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t => t.AdminStaffId == adminStaff.Id, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t=> t.BayId == bay.Id, cancellationToken);
        
        return trip;
    }
    
    public async Task<Trip?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t => t.WorkId == work.Id, cancellationToken);

        return trip;
    }
    
    public async Task<Trip?> GetAsync(Truck truck, CancellationToken cancellationToken)
    {
        var trip = await context.Trips
            .FirstOrDefaultAsync(t=> t.TruckId == truck.Id, cancellationToken);
        
        return trip;
    }
    
    public async Task<IQueryable<Trip>> GetCurrentAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await GetAsync(hub, cancellationToken))
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
    
    public async Task AddAsync(Trip trip, TripLog log, CancellationToken cancellationToken)
    {
        trip.TripLogs.Add(log);
        log.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldParkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
        if (oldParkingSpot != null)
            throw new Exception("Trip already has an assigned ParkingSpot, it cannot move to another.");
        
        trip.ParkingSpot = parkingSpot;
        parkingSpot.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var oldAdminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
        if (oldAdminStaff != null)
            throw new Exception("Trip already has an assigned AdminStaff, it cannot move to another.");
        
        trip.AdminStaff = adminStaff;
        adminStaff.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var oldBay = await bayRepository.GetAsync(trip, cancellationToken);
        if (oldBay != null)
            throw new Exception("Trip already has an assigned Bay, it cannot move to another.");
        
        trip.Bay = bay;
        bay.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        var oldTruck = await truckRepository.GetAsync(trip, cancellationToken);
        if (oldTruck != null)
            throw new Exception("Trip already has an assigned Truck, it cannot move to another.");
        
        trip.Truck = truck;
        truck.Trip = trip;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Trip trip, long xLocation, long yLocation, CancellationToken cancellationToken)
    {
        trip.XLocation = xLocation;
        trip.YLocation = yLocation;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UnsetAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        trip.ParkingSpot = null;
        parkingSpot.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UnsetAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        trip.AdminStaff = null;
        adminStaff.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UnsetAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        trip.Bay = null;
        bay.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UnsetAsync(Trip trip, Truck truck, CancellationToken cancellationToken)
    {
        trip.Truck = null;
        truck.Trip = null;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}