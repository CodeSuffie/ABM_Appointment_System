using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(
    ILogger<AdminStaffStepper> logger,
    AdminStaffService adminStaffService,
    WorkRepository workRepository,
    WorkService workService,
    AdminShiftService adminShiftService,
    AdminStaffRepository adminStaffRepository,
    ModelState modelState) : IStepperService<AdminStaff>
{
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(adminStaff, cancellationToken);
        
        if (work == null)
        {
            logger.LogInformation("AdminStaff ({@AdminStaff}) does not have active Work assigned in this Step ({Step}).",
                             adminStaff,
                             modelState.ModelTime);
            
            var shift = await adminShiftService.GetCurrentAsync(adminStaff, cancellationToken);
            if (shift == null)
            {
                logger.LogInformation("AdminStaff ({@AdminStaff}) is not working in this Step ({Step}).",
                    adminStaff,
                    modelState.ModelTime);
                
                logger.LogDebug("AdminStaff ({@AdminStaff}) will remain idle in this Step ({Step})...",
                    adminStaff,
                    modelState.ModelTime);
                
                return;
            }
            
            logger.LogDebug("Alerting Free for this AdminStaff ({@AdminStaff}) in this Step ({Step}).",
                adminStaff,
                modelState.ModelTime);
            await adminStaffService.AlertFreeAsync(adminStaff, cancellationToken);
            
            return;
        }
        
        if (workService.IsWorkCompleted(work))
        {
            logger.LogInformation("AdminStaff ({@AdminStaff}) just completed assigned Work ({@Work}) in this Step ({Step}).",
                adminStaff,
                work,
                modelState.ModelTime);
            
            logger.LogDebug("Alerting Work Completed for this AdminStaff ({@AdminStaff}) in this Step ({Step}).",
                adminStaff,
                modelState.ModelTime);
            await adminStaffService.AlertWorkCompleteAsync(adminStaff, cancellationToken);
        }
    }
    
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = adminStaffRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var adminStaff in adminStaffs)
        {
            logger.LogDebug("Handling Step ({Step}) for this AdminStaff ({@AdminStaff})...",
                modelState.ModelTime,
                adminStaff);
            
            await StepAsync(adminStaff, cancellationToken);
            
            logger.LogDebug("Completed handling Step ({Step}) for this AdminStaff ({@AdminStaff}).",
                modelState.ModelTime,
                adminStaff);
        }
    }
}