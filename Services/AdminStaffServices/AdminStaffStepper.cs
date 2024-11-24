using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    AdminStaffService adminStaffService,
    WorkRepository workRepository,
    WorkService workService,
    AdminShiftService adminShiftService,
    AdminStaffRepository adminStaffRepository,
    AdminStaffLogger adminStaffLogger) : IStepperService<AdminStaff>
{
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(adminStaff, cancellationToken);
        
        if (work == null)
        {
            var shift = await adminShiftService.GetCurrentAsync(adminStaff, cancellationToken);
            if (shift == null) return;
            await adminStaffLogger.LogAsync(adminStaff, LogType.Info, "Not working.", cancellationToken);
            
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
        var adminStaffs = (await adminStaffRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var adminStaff in adminStaffs)
        {
            await StepAsync(adminStaff, cancellationToken);
        }
    }
}