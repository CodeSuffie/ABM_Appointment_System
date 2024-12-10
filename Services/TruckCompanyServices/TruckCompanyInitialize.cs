using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.TruckCompanyServices;

public sealed class TruckCompanyInitialize(
    ILogger<TruckCompanyInitialize> logger,
    TruckCompanyService truckCompanyService,
    ModelState modelState) : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.High;
    private async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyService.GetNewObjectAsync(cancellationToken);
        logger.LogInformation("New TruckCompany created: TruckCompany={@TruckCompany}", truckCompany);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.TruckCompanyLocations.GetLength(0); i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}