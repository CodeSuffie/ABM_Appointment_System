using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckCompanyService(ModelDbContext context) : IAgentService<TruckCompany>
{
    private readonly ModelDbContext _context = context;

    public async Task InitializeAgentTrucksAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCountPerTruckCompany; i++)
        {
            truckCompany.Trucks.Add(new Truck
            {
                TruckCompany = truckCompany,
                Capacity = AgentConfig.TruckAverageCapacity
            });
        }
    }
    
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var hubs = context.Hubs.ToList();
        var hub = hubs[ModelConfig.Random.Next(hubs.Count)];

        var truckCompany = new TruckCompany();
        
        // TODO: Add Location
        
        await InitializeAgentTrucksAsync(truckCompany, cancellationToken);
        
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
        var truckCompanies = await _context.TruckCompanies.ToListAsync(cancellationToken);
        foreach (var truckCompany in truckCompanies)
        {
            await ExecuteStepAsync(truckCompany, cancellationToken);
        }
    }
}
