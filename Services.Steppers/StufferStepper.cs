using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class StufferStepper : IStepperService<Stuffer>
{
    private readonly ILogger<StufferStepper> _logger;
    private readonly HubShiftService _hubShiftService;
    private readonly WorkRepository _workRepository;
    private readonly WorkService _workService;
    private readonly StufferService _stufferService;
    private readonly StufferRepository _stufferRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _workingStufferHistogram;
    private readonly Histogram<int> _stuffingStufferHistogram;
    
    public StufferStepper(
        ILogger<StufferStepper> logger,
        HubShiftService hubShiftService,
        WorkRepository workRepository,
        WorkService workService,
        StufferService stufferService,
        StufferRepository stufferRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _hubShiftService = hubShiftService;
        _workRepository = workRepository;
        _workService = workService;
        _stufferService = stufferService;
        _stufferRepository = stufferRepository;
        _modelState = modelState;
        
        _workingStufferHistogram = meter.CreateHistogram<int>("working-stuffer", "Stuffer", "#Stuffer Working.");
        _stuffingStufferHistogram = meter.CreateHistogram<int>("fetch-stuffer", "Stuffer", "#Stuffer Working on a Stuff.");
    }
    
    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for Stuffer in this Step ({Step})", _modelState.ModelTime);
        
        // var working = await _stufferRepository.CountAsync(_modelState.ModelTime, cancellationToken);
        // _workingStufferHistogram.Record(working, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var fetching = await _stufferRepository.CountAsync(cancellationToken);
        // _stuffingStufferHistogram.Record(fetching, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for Stuffer in this Step ({Step})", _modelState.ModelTime);
    }
    
    public async Task StepAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(stuffer, cancellationToken);
        
        if (work == null)
        {
            _logger.LogInformation("Stuffer \n({@Stuffer})\n does not have active Work assigned in this Step ({Step})", stuffer, _modelState.ModelTime);
            
            var shift = await _hubShiftService.GetCurrentAsync(stuffer, cancellationToken);
            if (shift == null)
            {
                _logger.LogInformation("Stuffer \n({@Stuffer})\n is not working in this Step ({Step})", stuffer, _modelState.ModelTime);
                
                _logger.LogDebug("Stuffer \n({@Stuffer})\n will remain idle in this Step ({Step})", stuffer, _modelState.ModelTime);
                
                return;
            }
            
            _logger.LogDebug("Alerting Free for this Stuffer \n({@Stuffer})\n in this Step ({Step})", stuffer, _modelState.ModelTime);
            await _stufferService.AlertFreeAsync(stuffer, cancellationToken);
            
            return;
        }
        
        if (_workService.IsWorkCompleted(work))
        {
            _logger.LogInformation("Stuffer \n({@Stuffer})\n just completed assigned Work \n({@Work})\n in this Step ({Step})", stuffer, work, _modelState.ModelTime);
            
            _logger.LogDebug("Alerting Work Completed for this Stuffer \n({@Stuffer})\n in this Step ({Step})", stuffer, _modelState.ModelTime);
            await _stufferService.AlertWorkCompleteAsync(stuffer, cancellationToken);
            
            _logger.LogDebug("Removing old Work \n({@Work})\n for this Stuffer \n({@Stuffer})", work, stuffer);
            await _workRepository.RemoveAsync(work, cancellationToken);
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var stuffers = _stufferRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var stuffer in stuffers)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this Stuffer \n({@Stuffer})", _modelState.ModelTime, stuffer);
            
            await StepAsync(stuffer, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this Stuffer \n({@Stuffer})", _modelState.ModelTime, stuffer);
        }
    }
}