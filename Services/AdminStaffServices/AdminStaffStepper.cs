using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;
using Services.HubServices;
using Services.TripServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ModelDbContext context,
    HubRepository hubRepository,
    WorkRepository workRepository,
    AdminShiftRepository adminShiftRepository,
    TripService tripService,
    WorkService workService,
    TripRepository tripRepository) : IStepperService<AdminStaff>
{
     public async Task GetNewWorkAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
     {
         var shift = await adminShiftRepository.GetCurrentAsync(adminStaff, cancellationToken);
         if (shift == null) return;
                 
         var hub = await hubRepository.GetHubByStaffAsync(adminStaff, cancellationToken);
         
         var trip = await tripRepository.GetNextTripByHubByWorkTypeAsync(hub, WorkType.WaitCheckIn, cancellationToken);
         if (trip == null) return;
 
         await tripService.AlertFreeAsync(trip, adminStaff, cancellationToken);
     }
     
    public async Task WorkCompleteAsync(Work work, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetTripByWorkAsync(work, cancellationToken);
        if (trip == null)
            throw new Exception("The Work this BayStaff is doing is not being done for any Bay.");
                
        await tripService.AlertCheckInCompleteAsync(trip, cancellationToken);
        await workRepository.RemoveWorkAsync(work, cancellationToken);
    }
    
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetWorkByStaffAsync(adminStaff, cancellationToken);
        
        if (work == null)
        {
            await GetNewWorkAsync(adminStaff, cancellationToken);
            return;
        }
        
        if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            await WorkCompleteAsync(work, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = await context.AdminStaffs
            .ToListAsync(cancellationToken);
        
        foreach (var adminStaff in adminStaffs)
        {
            await StepAsync(adminStaff, cancellationToken);
        }
    }
}