using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class BayStaffService(
    ModelDbContext context, 
    BayShiftService bayShiftService
    ) : IInitializationService, IStepperService<BayStaff>
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = await context.Hubs
            .ToListAsync(cancellationToken);
        
        if (hubs.Count <= 0) 
            throw new Exception("There was no Hub to assign this new BayStaff to.");
        
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];
        
        var bayStaff = new BayStaff
        {
            Hub = hub
        };
        
        await bayShiftService.InitializeObjectsAsync(bayStaff, cancellationToken);
        
        context.BayStaffs
            .Add(bayStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = context.BayStaffs
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            await ExecuteStepAsync(bayStaff, cancellationToken);
        }
    }
}
