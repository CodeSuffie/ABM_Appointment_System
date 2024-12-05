using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class WorkRepository(
    ModelDbContext context,
    TripRepository tripRepository,
    AdminStaffRepository adminStaffRepository,
    BayStaffRepository bayStaffRepository,
    BayRepository bayRepository,
    PelletRepository pelletRepository)
{
    public IQueryable<Work> Get()
    {
        var works = context.Works;
        
        return works;
    }
    
    public IQueryable<Work> Get(Bay bay, WorkType workType)
    {
        var works = context.Works
            .Where(x => x.BayId == bay.Id && 
                        x.WorkType == workType);
        
        return works;
    }
    
    public async Task<Work?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        var work = await Get(bay, WorkType.Bay)
            .FirstOrDefaultAsync(cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.AdminStaff != null &&
                                      x.AdminStaffId == adminStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task<Work?> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await context.Works
            .FirstOrDefaultAsync(x => x.BayStaff != null &&
                                      x.BayStaffId == bayStaff.Id, cancellationToken);
        
        return work;
    }
    
    public async Task AddAsync(Work work, Trip trip, CancellationToken cancellationToken)
    {
        await context.Works
            .AddAsync(work, cancellationToken);
        
        work.Trip = trip;
        trip.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Work work, Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        await context.Works
            .AddAsync(work, cancellationToken);
        
        work.Trip = trip;
        trip.Work = work;

        work.AdminStaff = adminStaff;
        adminStaff.Work = work;

        context.Trips.Update(trip);
        context.AdminStaffs.Update(adminStaff);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Work work, Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        await context.Works
            .AddAsync(work, cancellationToken);
        
        work.Trip = trip;
        trip.Work = work;

        work.Bay = bay;
        bay.Works.Add(work);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task AddAsync(Work work, Bay bay, BayStaff bayStaff, Pellet pellet, CancellationToken cancellationToken)
    {
        work.Bay = bay;
        bay.Works.Add(work);

        work.BayStaff = bayStaff;
        bayStaff.Work = work;

        work.Pellet = pellet;
        pellet.Work = work;
        
        await context.Works
            .AddAsync(work, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task RemoveAsync(Work work, CancellationToken cancellationToken)
    {
        // var dbWork = await Get()
        //     .FirstOrDefaultAsync(w => w.Id == work.Id,
        //         cancellationToken);
        // if (dbWork == null)
        // {
        //     return;
        // }
        
        var trip = await tripRepository.GetAsync(work, cancellationToken);
        if (trip != null)
        {
            trip.Work = null;
        }
        
        var adminStaff = await adminStaffRepository.GetAsync(work, cancellationToken);
        if (adminStaff != null)
        {
            adminStaff.Work = null;
        }

        var bayStaff = await bayStaffRepository.GetAsync(work, cancellationToken);
        if (bayStaff != null)
        {
            bayStaff.Work = null;
        }
        
        var pellet = await pelletRepository.GetAsync(work, cancellationToken);
        if (pellet != null)
        {
            pellet.Work = null;
        }
        
        var bay = await bayRepository.GetAsync(work, cancellationToken);
        bay?.Works.Remove(work);

        context.Works
            .Remove(work);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDurationAsync(Work work, TimeSpan duration, CancellationToken cancellationToken)
    {
        work.Duration = duration;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}