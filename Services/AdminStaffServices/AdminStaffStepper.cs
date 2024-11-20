using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ModelDbContext context,
    AdminStaffService adminStaffService,
    WorkRepository workRepository,
    WorkService workService,
    AdminShiftService adminShiftService) : IStepperService<AdminStaff>
{
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(adminStaff, cancellationToken);
        
        if (work == null)
        {
            var shift = await adminShiftService.GetCurrentAsync(adminStaff, cancellationToken);
            if (shift == null) return;     // TODO: Log staff not working
            
            await adminStaffService.AlertFreeAsync(adminStaff, cancellationToken);
            return;
        }
        
        if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            await adminStaffService.AlertWorkCompleteAsync(adminStaff, cancellationToken);
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