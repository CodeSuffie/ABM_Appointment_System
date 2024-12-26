using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class StufferRepository(ModelDbContext context)
{
    public IQueryable<Stuffer> Get()
    {
        return context.Stuffers;
    }

    public Task<Stuffer?> GetAsync(Work work, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(pi => pi.Id == work.StufferId, cancellationToken);
    }

    public async Task AddAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        await context.Stuffers
            .AddAsync(stuffer, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(Stuffer stuffer, HubShift hubShift, CancellationToken cancellationToken)
    {
        stuffer.Shifts.Remove(hubShift);
        stuffer.Shifts.Add(hubShift);
        hubShift.Stuffer = stuffer;
        
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