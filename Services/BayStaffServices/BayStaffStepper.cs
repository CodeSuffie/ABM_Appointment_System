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
    public async Task GetNewWorkAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var shift = await bayShiftService.GetCurrentBayShiftForBayStaffAsync(bayStaff, cancellationToken);
        if (shift == null) return;

        var bay = await bayShiftService.GetBayForBayShiftAsync(shift, cancellationToken);
        if (bay == null)
            throw new Exception("This BayShift did not have a Bay assigned.");
                
        await bayService.AlertFreeAsync(bay, bayStaff, cancellationToken);
    }

    public async Task WorkCompleteAsync(Work work, CancellationToken cancellationToken)
    {
        var bay = await workService.GetBayForWorkAsync(work, cancellationToken);
        if (bay == null)
            throw new Exception("The Work this BayStaff is doing is not being done for any Bay.");
            
        await bayService.AlertBayWorkCompleteAsync(work.WorkType, bay, cancellationToken);
        await workService.RemoveWorkAsync(work, cancellationToken);
    }
    
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await bayStaffService.GetWorkForBayStaffAsync(bayStaff, cancellationToken);
        
        if (work == null)
        {
            await GetNewWorkAsync(bayStaff, cancellationToken);
            return;
        }
        
        if (await workService.IsWorkCompletedAsync(work, cancellationToken))
        {
            await WorkCompleteAsync(work, cancellationToken);
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