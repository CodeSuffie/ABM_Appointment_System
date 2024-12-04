using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TruckCompanyRepository(ModelDbContext context)
{
    public IQueryable<TruckCompany> Get()
    {
        var truckCompanies = context.TruckCompanies;

        return truckCompanies;
    }
    
    public async Task<TruckCompany?> GetAsync(Truck truck, CancellationToken cancellationToken)
    {
        var truckCompany = await Get()
            .FirstOrDefaultAsync(tc => tc.Id == truck.TruckCompanyId, cancellationToken);

        return truckCompany;
    }
    
    public async Task<TruckCompany?> GetAsync(Load load, CancellationToken cancellationToken)
    {
        if (load.TruckCompanyId == null)
        {
            return null;
        }
        
        var truckCompany = await Get()
            .FirstOrDefaultAsync(tc => tc.Id == load.TruckCompanyId, cancellationToken);

        return truckCompany;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        var count = await Get()
            .CountAsync(cancellationToken);

        return count;
    }
    
    public async Task AddAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await context.TruckCompanies
            .AddAsync(truckCompany, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}