using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyInitialize(
    ILogger<TruckCompanyInitialize> logger,
    TruckCompanyService truckCompanyService,
    LocationService locationService,
    TruckCompanyRepository truckCompanyRepository,
    ModelState modelState) : IPriorityInitializationService
{
    private async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = truckCompanyService.GetNewObject();
        
        logger.LogDebug("Setting location for this TruckCompany ({@TruckCompany})...",
            truckCompany);
        await locationService.InitializeObjectAsync(truckCompany, cancellationToken);
        
        // Initially no Trips are created

        await truckCompanyRepository.AddAsync(truckCompany, cancellationToken);
        logger.LogInformation("New TruckCompany created: TruckCompany={@TruckCompany}", truckCompany);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.TruckCompanyLocations.Length; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}