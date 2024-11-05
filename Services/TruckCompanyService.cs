using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckCompanyService(
    ModelDbContext context,
    LocationService locationService,
    TruckService truckService
    ) : IAgentService<TruckCompany>
{
    
    
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany();
        
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);
        await truckService.InitializeObjectsAsync(truckCompany, cancellationToken);
        
        context.TruckCompanies.Add(truckCompany);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCompanyCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = await context.TruckCompanies.ToListAsync(cancellationToken);
        foreach (var truckCompany in truckCompanies)
        {
            await ExecuteStepAsync(truckCompany, cancellationToken);
        }
    }
}
