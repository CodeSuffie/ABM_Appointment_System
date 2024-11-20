using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class TruckCompanyRepository(ModelDbContext context)
{
    public async Task<DbSet<TruckCompany>> GetAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies;

        return truckCompanies;
    }
    
    public async Task AddAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        await context.TruckCompanies
            .AddAsync(truckCompany, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }
}