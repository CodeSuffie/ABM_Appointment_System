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
    
    public async Task<TruckCompany> GetAsync(Truck truck, CancellationToken cancellationToken)
    {
        var truckCompany = await context.TruckCompanies
            .FirstOrDefaultAsync(tc => tc.Id == truck.TruckCompanyId, cancellationToken);
        if (truckCompany == null)
            throw new Exception("This Truck did not have a TruckCompany assigned.");

        return truckCompany;
    }
    
    public async Task AddAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await context.TruckCompanies
            .AddAsync(truckCompany, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}