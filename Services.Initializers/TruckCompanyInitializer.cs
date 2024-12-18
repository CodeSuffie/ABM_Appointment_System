using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class TruckCompanyInitializer(
    ILogger<TruckCompanyInitializer> logger,
    TruckCompanyFactory truckCompanyFactory,
    ModelState modelState) : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.High;
    private async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompany = await truckCompanyFactory.GetNewObjectAsync(cancellationToken);
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