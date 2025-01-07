using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.Steppers;

public sealed class PickerStepper : IStepperService<Picker>
{
    private readonly ILogger<PickerStepper> _logger;
    private readonly HubShiftService _hubShiftService;
    private readonly WorkRepository _workRepository;
    private readonly WorkService _workService;
    private readonly PickerService _pickerService;
    private readonly PickerRepository _pickerRepository;
    private readonly ModelState _modelState;
    private readonly Instrumentation _instrumentation;
    
    public PickerStepper(
        ILogger<PickerStepper> logger,
        HubShiftService hubShiftService,
        WorkRepository workRepository,
        WorkService workService,
        PickerService pickerService,
        PickerRepository pickerRepository,
        ModelState modelState,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _hubShiftService = hubShiftService;
        _workRepository = workRepository;
        _workService = workService;
        _pickerService = pickerService;
        _pickerRepository = pickerRepository;
        _modelState = modelState;
        _instrumentation = instrumentation;
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Picker in this Step ({Step})", _modelState.ModelTime);
        
        // var working = await _pickerRepository.CountAsync(_modelState.ModelTime, cancellationToken);
        // _workingPickerHistogram.Record(working, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var fetching = await _pickerRepository.CountAsync(cancellationToken);
        // _fetchingPickerHistogram.Record(fetching, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Picker in this Step ({Step})", _modelState.ModelTime);
    }
    
    public async Task StepAsync(Picker picker, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(picker, cancellationToken);
        
        if (work == null)
        {
            _logger.LogInformation("Picker \n({@Picker})\n does not have active Work assigned in this Step ({Step})", picker, _modelState.ModelTime);
            
            var shift = await _hubShiftService.GetCurrentAsync(picker, cancellationToken);
            if (shift == null)
            {
                _logger.LogInformation("Picker \n({@Picker})\n is not working in this Step ({Step})", picker, _modelState.ModelTime);
                
                _logger.LogDebug("Picker \n({@Picker})\n will remain idle in this Step ({Step})", picker, _modelState.ModelTime);
                
                return;
            }
            
            _logger.LogDebug("Alerting Free for this Picker \n({@Picker})\n in this Step ({Step})", picker, _modelState.ModelTime);
            await _pickerService.AlertFreeAsync(picker, cancellationToken);
            
            _instrumentation.WorkingPickerCounter.Add(1, 
            [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("Picker", picker.Id)
            ]);
            
            return;
        }
        
        _instrumentation.WorkingPickerCounter.Add(1, 
        [
            new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
            new KeyValuePair<string, object?>("Picker", picker.Id)
        ]);
        
        if (_workService.IsWorkCompleted(work))
        {
            _logger.LogInformation("Picker \n({@Picker})\n just completed assigned Work \n({@Work})\n in this Step ({Step})", picker, work, _modelState.ModelTime);
            
            _logger.LogDebug("Alerting Work Completed for this Picker \n({@Picker})\n in this Step ({Step})", picker, _modelState.ModelTime);
            await _pickerService.AlertWorkCompleteAsync(picker, cancellationToken);
            
            _logger.LogDebug("Removing old Work \n({@Work})\n for this Picker \n({@Picker})", work, picker);
            await _workRepository.RemoveAsync(work, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var pickers = _pickerRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var picker in pickers)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this Picker \n({@Picker})", _modelState.ModelTime, picker);
            
            await StepAsync(picker, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this Picker \n({@Picker})", _modelState.ModelTime, picker);
        }
    }
}