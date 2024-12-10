using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.PickerServices;

public sealed class PickerStepper : IStepperService<Picker>
{
    private readonly ILogger<PickerStepper> _logger;
    private readonly PickerRepository _pickerRepository;
    private readonly ModelState _modelState;
    
    public PickerStepper(
        ILogger<PickerStepper> logger,
        PickerRepository pickerRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pickerRepository = pickerRepository;
        _modelState = modelState;
    }

    public Task DataCollectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public Task StepAsync(Picker picker, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var pickers = _pickerRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var picker in pickers)
        {
            _logger.LogDebug("Handling Step \n({Step})\n for this Picker \n({@Picker})",
                _modelState.ModelTime,
                picker);
            
            await StepAsync(picker, cancellationToken);
            
            _logger.LogDebug("Completed handling Step \n({Step})\n for this Picker \n({@Picker})",
                _modelState.ModelTime,
                picker);
        }
    }
}