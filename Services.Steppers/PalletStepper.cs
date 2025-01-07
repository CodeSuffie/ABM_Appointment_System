using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Factories;

namespace Services.Steppers;

public sealed class PalletStepper : IStepperService
{
    private readonly ILogger<PalletStepper> _logger;
    private readonly PalletFactory _palletFactory;
    private readonly ModelState _modelState;
    
    public PalletStepper(
        ILogger<PalletStepper> logger,
        PalletFactory palletFactory,
        ModelState modelState)
    {
        _logger = logger;
        _palletFactory = palletFactory;
        _modelState = modelState;
    }
    
    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        await _palletFactory.AddNewTruckCompanyPalletsAsync(_modelState.ModelConfig.PalletsPerStep, cancellationToken);
    }
}