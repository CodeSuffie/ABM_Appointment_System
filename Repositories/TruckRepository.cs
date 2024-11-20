using Database;
using Database.Models;

namespace Repositories;

public sealed class TruckRepository(ModelDbContext context)
{
    public async Task AddAsync(Truck truck, CancellationToken cancellationToken)
    {
        await context.Trucks
            .AddAsync(truck, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}