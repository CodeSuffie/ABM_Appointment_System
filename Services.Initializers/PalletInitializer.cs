using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class PalletInitializer  : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;

    private readonly ILogger<PalletInitializer> _logger;
    private readonly PalletFactory _palletFactory;
    private readonly ModelState _modelState;

    public PalletInitializer(
        ILogger<PalletInitializer> logger,
        ModelState modelState,
        PalletFactory palletFactory)
    {
        _logger = logger;
        _modelState = modelState;
        _palletFactory = palletFactory;
    }
    
    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await _palletFactory.AddNewTruckCompanyPalletsAsync(_modelState.ModelConfig.InitialTruckCompanyPallets, cancellationToken);
        await _palletFactory.AddNewWarehousePalletsAsync(_modelState.ModelConfig.InitialWarehousePallets, cancellationToken);
    }
}