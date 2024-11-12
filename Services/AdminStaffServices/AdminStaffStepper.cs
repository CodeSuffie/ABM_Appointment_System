using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Services.HubServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ModelDbContext context,
    AdminStaffService adminStaffService,
    HubService hubService) : IStepperService<AdminStaff>
{
    public async Task AlertFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await adminStaffService.GetHubForAdminStaffAsync(adminStaff, cancellationToken);
        
        var trip = await hubService.GetNextCheckInTripAsync(hub, cancellationToken);
        if (trip == null) return;
        
        // TODO: Build workService.AddWorkAsync(adminStaff, trip, cancellationToken)
    }

    public async Task<bool> CheckFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await adminStaffService.GetWorkForAdminStaffAsync(adminStaff, cancellationToken);
        return work == null;
    }

    public async Task<bool> CheckWorkingAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        // TODO: If no active shift, wait
        return true;
    }
    
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        // TODO: If not free, continue handling Check-In (in WorkService)

        if (!await CheckWorkingAsync(adminStaff, cancellationToken)) return;

        if (!await CheckFreeAsync(adminStaff, cancellationToken)) return;
        
        await AlertFreeAsync(adminStaff, cancellationToken);
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