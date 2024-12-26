using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TruckCompanyRepository(ModelDbContext context)
{
    public IQueryable<TruckCompany> Get()
    {
        return context.TruckCompanies;
    }
    
    public Task<TruckCompany?> GetAsync(Truck truck, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(tc => tc.Id == truck.TruckCompanyId, cancellationToken);
    }
    
    public Task<TruckCompany?> GetAsync(Load load, CancellationToken cancellationToken)
    {
        return Get()
            .FirstOrDefaultAsync(tc => tc.Id == load.TruckCompanyId, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return Get()
            .CountAsync(cancellationToken);
    }
    
    public async Task AddAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await context.TruckCompanies
            .AddAsync(truckCompany, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}