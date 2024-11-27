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
            logger.LogInformation("AdminStaff \n({@AdminStaff})\n does not have active Work assigned in this Step \n({Step})",
                             adminStaff,
                             modelState.ModelTime);
            
            var shift = await adminShiftService.GetCurrentAsync(adminStaff, cancellationToken);
            if (shift == null)
            {
                logger.LogInformation("AdminStaff \n({@AdminStaff})\n is not working in this Step \n({Step})",
                    adminStaff,
                    modelState.ModelTime);
                
                logger.LogDebug("AdminStaff \n({@AdminStaff})\n will remain idle in this Step \n({Step})",
                    adminStaff,
                    modelState.ModelTime);
                
                return;
            }
            
            logger.LogDebug("Alerting Free for this AdminStaff \n({@AdminStaff})\n in this Step \n({Step})",
                adminStaff,
                modelState.ModelTime);
            await adminStaffService.AlertFreeAsync(adminStaff, cancellationToken);
            
            return;
        }
        
        if (workService.IsWorkCompleted(work))
        {
            logger.LogInformation("AdminStaff \n({@AdminStaff})\n just completed assigned Work \n({@Work})\n in this Step \n({Step})",
                adminStaff,
                work,
                modelState.ModelTime);
            
            logger.LogDebug("Alerting Work Completed for this AdminStaff \n({@AdminStaff})\n in this Step \n({Step})",
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
            logger.LogDebug("Handling Step \n({Step})\n for this AdminStaff \n({@AdminStaff})",
                modelState.ModelTime,
                adminStaff);
            
            await StepAsync(adminStaff, cancellationToken);
            
            logger.LogDebug("Completed handling Step \n({Step})\n for this AdminStaff \n({@AdminStaff})",
                modelState.ModelTime,
                adminStaff);
        }
    }
}