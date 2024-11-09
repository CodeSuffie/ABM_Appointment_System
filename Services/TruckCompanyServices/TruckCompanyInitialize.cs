using Database;
using Services.Abstractions;
using Settings;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyInitialize(
    ModelDbContext context,
    TruckCompanyService truckCompanyService,
    LocationService locationService) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.GetNewObjectAsync(cancellationToken);
        
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
}