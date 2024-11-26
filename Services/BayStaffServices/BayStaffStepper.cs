using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.BayStaffServices;

public sealed class BayStaffStepper(
    ILogger<BayStaffStepper> logger,
    WorkRepository workRepository,
    BayRepository bayRepository,
    BayShiftService bayShiftService,
    WorkService workService,
    BayStaffService bayStaffService,
    BayStaffRepository bayStaffRepository,
    ModelState modelState) : IStepperService<BayStaff>
{
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(bayStaff, cancellationToken);
        
        if (work == null)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) does not have active Work assigned in this Step ({Step}).",
                         bayStaff,
                         modelState.ModelTime);
            
            var shift = await bayShiftService.GetCurrentAsync(bayStaff, cancellationToken);
            if (shift == null)
            {
                logger.LogInformation("BayStaff ({@BayStaff}) is not working in this Step ({Step}).",
                    bayStaff,
                    modelState.ModelTime);
                
                logger.LogDebug("BayStaff ({@BayStaff}) will remain idle in this Step ({Step})...",
                    bayStaff,
                    modelState.ModelTime);
                
                return;
            }
            
            var bay = await bayRepository.GetAsync(shift, cancellationToken);
            if (bay == null)
            {
                logger.LogError("The current BayShift ({@BayShift}) for this BayStaff ({@BayStaff}) did not have a Bay assigned.",
                    shift,
                    bayStaff);
                
                logger.LogDebug("BayStaff ({@BayStaff}) will remain idle in this Step ({Step})...",
                    bayStaff,
                    modelState.ModelTime);
                
                return;
            }
            
            logger.LogDebug("Alerting Free for this BayStaff ({@BayStaff}) in this Step ({Step}).",
                bayStaff,
                modelState.ModelTime);
            await bayStaffService.AlertFreeAsync(bayStaff, bay, cancellationToken);
            
            return;
        }
        
        if (workService.IsWorkCompleted(work))
        {
            logger.LogInformation("BayStaff ({@BayStaff}) just completed assigned Work ({@Work}) in this Step ({Step}).",
                bayStaff,
                work,
                modelState.ModelTime);
            
            var bay = await bayRepository.GetAsync(work, cancellationToken);
            if (bay == null)
            {
                logger.LogError("The active assigned Work ({@Work}) for this BayStaff ({@BayStaff}) did not have a Bay assigned.",
                    work,
                    bayStaff);

                logger.LogDebug("Removing invalid Work ({@Work}) for this BayStaff ({@BayStaff}).",
                    work,
                    bayStaff);
                await workRepository.RemoveAsync(work, cancellationToken);
                
                logger.LogDebug("BayStaff ({@BayStaff}) will remain idle in this Step ({Step})...",
                    bayStaff,
                    modelState.ModelTime);

                return;
            }

            logger.LogDebug("Alerting Work Completed for this BayStaff ({@BayStaff}) in this Step ({Step})...",
                bayStaff,
                modelState.ModelTime);
            await bayStaffService.AlertWorkCompleteAsync(work.WorkType, bay, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = bayStaffRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            logger.LogDebug("Handling Step ({Step}) for this BayStaff ({@BayStaff})...",
                modelState.ModelTime,
                bayStaff);
            
            await StepAsync(bayStaff, cancellationToken);
            
            logger.LogDebug("Completed handling Step ({Step}) for this BayStaff ({@BayStaff}).",
                modelState.ModelTime,
                bayStaff);
        }
    }
}