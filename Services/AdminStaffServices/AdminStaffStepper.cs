using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Services.HubServices;
using Services.TripServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ModelDbContext context,
    AdminStaffService adminStaffService,
    AdminShiftService adminShiftService,
    TripService tripService,
    WorkService workService,
    HubService hubService) : IStepperService<AdminStaff>
{
    public async Task AlertFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await adminStaffService.GetHubForAdminStaffAsync(adminStaff, cancellationToken);
        
        var trip = await hubService.GetNextCheckInTripAsync(hub, cancellationToken);
        if (trip == null) return;

        await tripService.AlertFreeAsync(trip, adminStaff, cancellationToken);
    }
    
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await adminStaffService.GetWorkForAdminStaffAsync(adminStaff, cancellationToken);
        
        if (work != null)
        {
            if (await workService.IsWorkCompletedAsync(work, cancellationToken))
            {
                var trip = await workService.GetTripForWorkAsync(work, cancellationToken);
                if (trip == null)
                    throw new Exception("The Work this BayStaff is doing is not being done for any Bay.");
                
                await tripService.AlertCheckInCompleteAsync(trip, cancellationToken);
                await workService.RemoveWorkAsync(work, cancellationToken);
            }
        }
        else
        {
            var shift = await adminShiftService.GetCurrentShiftAsync(adminStaff, cancellationToken);
            if (shift == null) return;
                
            await AlertFreeAsync(adminStaff, cancellationToken);
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