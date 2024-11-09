using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;

namespace Services.AdminStaffServices;

public sealed class AdminStaffStepper(ModelDbContext context) : IStepperService<AdminStaff>
{
    public async Task ExecuteStepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = await context.AdminStaffs
            .ToListAsync(cancellationToken);
        
        foreach (var adminStaff in adminStaffs)
        {
            await ExecuteStepAsync(adminStaff, cancellationToken);
        }
    }
}