using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Services.HubServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ModelDbContext context,
    AdminStaffService adminStaffService,
    AdminShiftService adminShiftService,
    WorkService workService,
    HubService hubService) : IStepperService<AdminStaff>
{
    public async Task<bool> IsWorkingAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shift = await adminShiftService.GetCurrentShiftAsync(adminStaff, cancellationToken);
        return shift != null;
    }
    
    public async Task AlertFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await adminStaffService.GetHubForAdminStaffAsync(adminStaff, cancellationToken);
        
        var trip = await hubService.GetNextCheckInTripAsync(hub, cancellationToken);
        if (trip == null) return;

        await workService.AddWorkAsync(adminStaff, trip, cancellationToken);
    }
    
    public async Task HandleWorkAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await adminStaffService.GetWorkForAdminStaffAsync(adminStaff, cancellationToken);
        if (work == null)
        {
            await AlertFreeAsync(adminStaff, cancellationToken);
        }
        else if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            await workService.RemoveWorkAsync(work, cancellationToken);
        }
    }
    
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        // TODO: Fix cases where adminStaff has work assigned at end of shift
        if (!await IsWorkingAsync(adminStaff, cancellationToken)) return;

        await HandleWorkAsync(adminStaff, cancellationToken);
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