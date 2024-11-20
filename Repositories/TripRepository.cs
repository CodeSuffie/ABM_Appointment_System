using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TripRepository(
    ModelDbContext context,
    BayRepository bayRepository,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository)
{
    public async Task<IQueryable<Trip>> GetAsync(Hub hub, CancellationToken cancellationToken)
    {
        var trips = context.Trips
            .Where(t => t.HubId == hub.Id);

        return trips;
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
    
    public async Task<IQueryable<Trip>> GetCurrentAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await GetAsync(hub, cancellationToken))
            .Where(t => t.Work != null &&
                        t.Work.WorkType == workType);
        
        return trips;
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
}