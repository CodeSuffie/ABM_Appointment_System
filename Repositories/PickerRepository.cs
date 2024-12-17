using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class PickerRepository(ModelDbContext context)
{
    public IQueryable<Picker> Get()
    {
        var pickers = context.Pickers;

        return pickers;
    }

    public async Task<Picker?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        var picker = await Get()
            .FirstOrDefaultAsync(pi => pi.Id == work.PickerId, cancellationToken);

        return picker;
    }

    public async Task AddAsync(Picker picker, CancellationToken cancellationToken)
    {
        await context.Pickers
            .AddAsync(picker, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Picker picker, HubShift hubShift, CancellationToken cancellationToken)
    {
        picker.Shifts.Remove(hubShift);
        picker.Shifts.Add(hubShift);
        hubShift.Picker = picker;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(TimeSpan time, CancellationToken cancellationToken)
    {
        return Get()
            .Where(pi => pi.Shifts
                .Any(sh => sh.StartTime <= time && 
                           sh.StartTime + sh.Duration >= time))
            .CountAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return Get()
            .Where(pi => pi.Work != null)
            .CountAsync(cancellationToken);
    }
}