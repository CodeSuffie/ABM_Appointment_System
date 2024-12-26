using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class WorkRepository(
    ModelDbContext context,
    TripRepository tripRepository,
    AdminStaffRepository adminStaffRepository,
    BayStaffRepository bayStaffRepository,
    PickerRepository pickerRepository,
    StufferRepository stufferRepository,
    BayRepository bayRepository,
    PelletRepository pelletRepository)
{
    public IQueryable<Work> Get()
    {
        return context.Works;
    }
    
    public IQueryable<Work> Get(Bay bay, WorkType workType)
    {
        return Get()
            .Where(x => x.BayId == bay.Id && 
                        x.WorkType == workType);
    }
    
    public Task<Work?> GetAsync(Trip trip, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.TripId == trip.Id, cancellationToken);
    }
    
    public Task<Work?> GetAsync(Bay bay, CancellationToken cancellationToken)
    {
        return Get(bay, WorkType.Bay)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public Task<Work?> GetAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.AdminStaff != null &&
                                      x.AdminStaffId == adminStaff.Id, cancellationToken);
    }
    
    public Task<Work?> GetAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.BayStaff != null &&
                                      x.BayStaffId == bayStaff.Id, cancellationToken);
    }

    public Task<Work?> GetAsync(Picker picker, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.Picker != null &&
                                      x.PickerId == picker.Id, cancellationToken);
    }
    
    public Task<Work?> GetAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.Stuffer != null &&
                                      x.StufferId == stuffer.Id, cancellationToken);
    }

    public Task<Work?> GetAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(x => x.Pellet != null &&
                                      x.PelletId == pellet.Id, cancellationToken);
    }

    public async Task AddAsync(Work work, CancellationToken cancellationToken)
    {
        await context.Works.AddAsync(work, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, WorkType workType, CancellationToken cancellationToken)
    {
        work.WorkType = workType;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, Trip trip, CancellationToken cancellationToken)
    {
        work.Trip = trip;
        trip.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        work.AdminStaff = adminStaff;
        adminStaff.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        work.BayStaff = bayStaff;
        bayStaff.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, Picker picker, CancellationToken cancellationToken)
    {
        work.Picker = picker;
        picker.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, Stuffer stuffer, CancellationToken cancellationToken)
    {
        work.Stuffer = stuffer;
        stuffer.Work = work;

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, Bay bay, CancellationToken cancellationToken)
    {
        work.Bay = bay;
        bay.Works.Remove(work);
        bay.Works.Add(work);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task SetAsync(Work work, Pellet pellet, CancellationToken cancellationToken)
    {
        work.Pellet = pellet;
        pellet.Work = work;

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
        
        var picker = await pickerRepository.GetAsync(work, cancellationToken);
        if (picker != null)
        {
            picker.Work = null;
        }
        
        var stuffer = await stufferRepository.GetAsync(work, cancellationToken);
        if (stuffer != null)
        {
            stuffer.Work = null;
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

    public async Task SetDurationAsync(Work work, TimeSpan? duration, CancellationToken cancellationToken)
    {
        work.Duration = duration;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}