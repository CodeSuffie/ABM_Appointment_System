using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Steppers;

public sealed class PelletStepper : IStepperService
{
    private readonly ILogger<PelletStepper> _logger;
    private readonly PelletFactory _pelletFactory;
    private readonly ModelState _modelState;
    
    public PelletStepper(
        ILogger<PelletStepper> logger,
        PelletFactory pelletFactory,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletFactory = pelletFactory;
        _modelState = modelState;
    }
    
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await _pelletFactory.AddNewTruckCompanyPelletsAsync(_modelState.ModelConfig.PelletsPerStep, cancellationToken);
    }
}