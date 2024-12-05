using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services.PelletServices;

public sealed class PelletInitialize : IInitializationService
{
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
        await _pelletCreation.AddNewBayPelletsAsync(_modelState.ModelConfig.InitialBayPellets, cancellationToken);
    }
}