using System.Globalization;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckDriverService(ModelDbContext context) : IAgentService<TruckDriver>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies.ToList();
        var truckCompany = truckCompanies[ModelConfig.Random.Next(truckCompanies.Count)];
        
        var truckDriver = new TruckDriver
        {
            TruckCompany = truckCompany
        };
        
        await TruckShiftService.InitializeObjectsAsync(truckDriver, cancellationToken);
        
        context.TruckDrivers.Add(truckDriver);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckDriverCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteStepAsync(TruckDriver truckDriver, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var truckDrivers = await context.TruckDrivers.ToListAsync(cancellationToken);
        foreach (var truckDriver in truckDrivers)
        {
            await ExecuteStepAsync(truckDriver, cancellationToken);
        }
    }
}
