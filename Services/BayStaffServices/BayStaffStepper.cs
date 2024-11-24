using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.BayStaffServices;

public sealed class BayStaffStepper(
    WorkRepository workRepository,
    BayRepository bayRepository,
    BayShiftService bayShiftService,
    WorkService workService,
    BayStaffService bayStaffService,
    BayStaffRepository bayStaffRepository,
    BayStaffLogger bayStaffLogger) : IStepperService<BayStaff>
{
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(bayStaff, cancellationToken);
        
        if (work == null)
        {
            var shift = await bayShiftService.GetCurrentAsync(bayStaff, cancellationToken);
            if (shift == null)
            {
                await bayStaffLogger.LogAsync(bayStaff, LogType.Info, "Not working.", cancellationToken);
                return;
            }
            
            var bay = await bayRepository.GetAsync(shift, cancellationToken);
            if (bay == null)
                throw new Exception("This BayShift did not have a Bay assigned.");
            
            await bayStaffService.AlertFreeAsync(bayStaff, bay, cancellationToken);
            return;
        }
        if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            var bay = await bayRepository.GetAsync(work, cancellationToken);
            if (bay == null)
                throw new Exception("This Work did not have a Bay assigned.");

            await bayStaffService.AlertWorkCompleteAsync(work.WorkType, bay, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = (await bayStaffRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            await StepAsync(bayStaff, cancellationToken);
        }
    }
}