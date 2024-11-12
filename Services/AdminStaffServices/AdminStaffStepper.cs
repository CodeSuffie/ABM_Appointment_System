using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(ModelDbContext context) : IStepperService<AdminStaff>
{
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: If no active shift, wait
        // TODO: If free, and no waiting Truck, wait
        // TODO: Otherwise alert next Truck I am free
        // TODO: If not free, continue handling Check-In
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