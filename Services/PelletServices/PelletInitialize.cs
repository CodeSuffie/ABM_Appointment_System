using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletInitialize(
    ILogger<PelletInitialize> logger,
    PelletService pelletService,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await pelletService.AddNewTruckCompanyPelletsAsync(modelState.ModelConfig.InitialTruckCompanyPellets, cancellationToken);
        await pelletService.AddNewBayPelletsAsync(modelState.ModelConfig.InitialBayPellets, cancellationToken);
    }
}