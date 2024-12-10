using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletStepper : IStepperService
{
    private readonly ILogger<PelletStepper> _logger;
    private readonly PelletCreation _pelletCreation;
    private readonly ModelState _modelState;
    
    public PelletStepper(
        ILogger<PelletStepper> logger,
        PelletCreation pelletCreation,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletCreation = pelletCreation;
        _modelState = modelState;
    }
    
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await _pelletCreation.AddNewTruckCompanyPelletsAsync(_modelState.ModelConfig.PelletsPerStep, cancellationToken);
    }
}