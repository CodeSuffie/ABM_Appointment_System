using Database;
using Database.Models;
using Services.Abstractions;
using Services.BayServices;
using Services.HubServices;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffStepper(
    ModelDbContext context,
    BayStaffService bayStaffService,
    BayShiftService bayShiftService,
    WorkService workService,
    BayService bayService) : IStepperService<BayStaff>
{
    public async Task AlertBayAvailabilityAsync(BayShift bayShift, Bay bay, CancellationToken cancellationToken)
    {
        if (!bay.Opened)
        {
            await bayService.AlertShiftStartAsync(bay, cancellationToken);
        }

        if (await bayShiftService.DoesShiftEndInAsync(bayShift, 1 * ModelConfig.ModelStep, cancellationToken))
        {
            await bayService.AlertShiftEndAsync(bay, cancellationToken);
        }
    }
    
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        // TODO: Fix cases where bayStaff has work assigned at end of shift
        
        var shift = await bayShiftService.GetCurrentShiftAsync(bayStaff, cancellationToken);
        if (shift == null) return;
        
        var bay = await bayShiftService.GetBayForBayShiftAsync(shift, cancellationToken);
        if (bay == null)
            throw new Exception("This BayShift did not have a Bay assigned.");

        await AlertBayAvailabilityAsync(shift, bay, cancellationToken);
        
        var work = await bayStaffService.GetWorkForBayStaffAsync(bayStaff, cancellationToken);
        if (work == null)
        {
            await bayService.AlertFreeAsync(bay, bayStaff, cancellationToken);
        }
        else if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            await workService.RemoveWorkAsync(work, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = context.BayStaffs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            await StepAsync(bayStaff, cancellationToken);
        }
    }
}