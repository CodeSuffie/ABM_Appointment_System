using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletInitialize  : IPriorityInitializationService
{
    public Priority Priority { get; set; } = Priority.Low;

    private readonly ILogger<PelletInitialize> _logger;
    private readonly PelletFactory _pelletFactory;
    private readonly ModelState _modelState;

    public PelletInitialize(
        ILogger<PelletInitialize> logger,
        ModelState modelState,
        PelletFactory pelletFactory,
        Meter meter)
    {
        _logger = logger;
        _modelState = modelState;
        _pelletFactory = pelletFactory;
    }
    
    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        await _pelletFactory.AddNewTruckCompanyPelletsAsync(_modelState.ModelConfig.InitialTruckCompanyPellets, cancellationToken);
        await _pelletFactory.AddNewWarehousePelletsAsync(_modelState.ModelConfig.InitialWarehousePellets, cancellationToken);
    }
}