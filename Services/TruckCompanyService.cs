using Database;
using Database.Models;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class TruckCompanyService(
    ModelDbContext context,
    LocationService locationService
    ) : IInitializationService, IStepperService<TruckCompany>
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = new TruckCompany();
        
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);
        // Initially no Trips are created
        
        context.TruckCompanies
            .Add(truckCompany);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.TruckCompanyCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var truckCompanies = context.TruckCompanies
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var truckCompany in truckCompanies)
        {
            await ExecuteStepAsync(truckCompany, cancellationToken);
        }
    }
}
