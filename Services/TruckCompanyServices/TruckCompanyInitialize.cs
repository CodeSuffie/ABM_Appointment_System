using Repositories;
using Services.Abstractions;
using Services.ModelServices;
using Settings;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyInitialize(
    TruckCompanyService truckCompanyService,
    LocationService locationService,
    TruckCompanyRepository truckCompanyRepository,
    ModelState modelState) : IPriorityInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.GetNewObjectAsync(cancellationToken);
        
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);
        // Initially no Trips are created

        await truckCompanyRepository.AddAsync(truckCompany, cancellationToken);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.TruckCompanyCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}