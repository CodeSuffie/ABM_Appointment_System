using Database;
using Database.Models;

namespace Repositories;

public sealed class PickerRepository(ModelDbContext context)
{
    public IQueryable<Picker> Get()
    {
        var pickers = context.Pickers;

        return pickers;
    }

    public async Task AddAsync(Picker picker, CancellationToken cancellationToken)
    {
        await context.Pickers
            .AddAsync(picker, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Picker picker, HubShift hubShift, CancellationToken cancellationToken)
    {
        picker.Shifts.Add(hubShift);
        hubShift.Picker = picker;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}