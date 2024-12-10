using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletInitialize  : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Low;

    private readonly ILogger<PelletInitialize> _logger;
    private readonly PelletCreation _pelletCreation;
    private readonly ModelState _modelState;

    public PelletInitialize(
        ILogger<PelletInitialize> logger,
        ModelState modelState,
        PelletCreation pelletCreation,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _pelletCreation = pelletCreation;
    }
    
    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await _pelletCreation.AddNewTruckCompanyPelletsAsync(_modelState.ModelConfig.InitialTruckCompanyPellets, cancellationToken);
        await _pelletCreation.AddNewWarehousePelletsAsync(_modelState.ModelConfig.InitialWarehousePellets, cancellationToken);
    }
}