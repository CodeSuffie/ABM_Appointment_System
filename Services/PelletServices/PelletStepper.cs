using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletStepper : IStepperService
{
    private readonly ILogger<PelletStepper> _logger;
    private readonly PelletService _pelletService;
    private readonly PelletRepository _pelletRepository;
    private readonly ModelState _modelState;
    
    public PelletStepper(
        ILogger<PelletStepper> logger,
        PelletRepository pelletRepository,
        PelletService pelletService,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _pelletService = pelletService;
        _modelState = modelState;
    }
    
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await _pelletService.AddNewTruckCompanyPelletsAsync(_modelState.ModelConfig.LoadsPerStep, cancellationToken);
    }
}