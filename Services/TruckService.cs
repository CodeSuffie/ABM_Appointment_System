using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckService(ModelDbContext context) : IInitializationService, IStepperService<Truck>
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = await context.TruckCompanies
            .ToListAsync(cancellationToken);
        
        if (truckCompanies.Count <= 0) 
            throw new Exception("There was no Truck Company to assign this new Truck to.");
        
        var truckCompany = truckCompanies[ModelConfig.Random.Next(truckCompanies.Count)];
        
        var truck = new Truck
        {
            TruckCompany = truckCompany,
            Capacity = AgentConfig.TruckAverageCapacity,
            Planned = false
        };
        
        context.Trucks
            .Add(truck);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Truck truck, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var trucks = context.Trucks
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truck in trucks)
        {
            await ExecuteStepAsync(truck, cancellationToken);
        }
    }
}