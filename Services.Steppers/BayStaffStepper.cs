using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Settings;

namespace Services.Steppers;

public sealed class BayStaffStepper : IStepperService<BayStaff>
{
    private readonly ILogger<BayStaffStepper> _logger;
    private readonly BayRepository _bayRepository;
    private readonly BayShiftService _bayShiftService;
    private readonly WorkRepository _workRepository;
    private readonly WorkService _workService;
    private readonly BayStaffService _bayStaffService;
    private readonly BayStaffRepository _bayStaffRepository;
    private readonly ModelState _modelState;
    private readonly Instrumentation _instrumentation;
    
    public BayStaffStepper(
        ILogger<BayStaffStepper> logger,
        BayRepository bayRepository,
        BayShiftService bayShiftService,
        WorkRepository workRepository,
        WorkService workService,
        BayStaffService bayStaffService,
        BayStaffRepository bayStaffRepository,
        ModelState modelState,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _bayRepository = bayRepository;
        _bayShiftService = bayShiftService;
        _workRepository = workRepository;
        _workService = workService;
        _bayStaffService = bayStaffService;
        _bayStaffRepository = bayStaffRepository;
        _modelState = modelState;
        _instrumentation = instrumentation;
    }
    
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(bayStaff, cancellationToken);
        var shift = await _bayShiftService.GetCurrentAsync(bayStaff, cancellationToken);
        
        if (work == null)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n does not have active Work assigned in this Step ({Step})", bayStaff, _modelState.ModelTime);
            
            if (shift == null)
            {
                _logger.LogInformation("BayStaff \n({@BayStaff})\n is not working in this Step ({Step})", bayStaff, _modelState.ModelTime);
                
                _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);
            }
            else
            {
                var bay = await _bayRepository.GetAsync(shift, cancellationToken);
                if (bay == null)
                {
                    _logger.LogError("The current BayShift \n({@BayShift})\n for this BayStaff \n({@BayStaff})\n did not have a Bay assigned.", shift, bayStaff);
                
                    _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);
                
                }
                else
                {
                    _logger.LogDebug("Alerting Free for this BayStaff \n({@BayStaff})\n in this Step ({Step})", bayStaff, _modelState.ModelTime);
                    await _bayStaffService.AlertFreeAsync(bayStaff, bay, cancellationToken);
                }
            }
        }
        else if (_workService.IsWorkCompleted(work))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n just completed assigned Work \n({@Work})\n in this Step ({Step})", bayStaff, work, _modelState.ModelTime);
            
            var bay = await _bayRepository.GetAsync(work, cancellationToken);
            if (bay == null)
            {
                _logger.LogError("The active assigned Work \n({@Work})\n for this BayStaff \n({@BayStaff})\n did not have a Bay assigned.", work, bayStaff);
                
                _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);
            }
            else
            {
                _logger.LogDebug("Alerting Work Completed for this BayStaff \n({@BayStaff})\n in this Step ({Step})", bayStaff, _modelState.ModelTime);
                await _bayStaffService.AlertWorkCompleteAsync(work, bay, bayStaff, cancellationToken);
            
                _logger.LogDebug("Removing old Work \n({@Work})\n for this BayStaff \n({@BayStaff})", work, bayStaff);
                await _workRepository.RemoveAsync(work, cancellationToken);
                
                
            }
        }
        
        if (shift == null) return;

        if (shift.StartTime == _modelState.ModelTime)
        {
            _instrumentation.Add(Metric.BayStaffWorking, 1, ("BayStaff", bayStaff.Id));
        }

        if (shift.StartTime + shift.Duration == _modelState.ModelTime + _modelState.ModelConfig.ModelStep)
        {
            _instrumentation.Add(Metric.BayStaffWorking, -1, ("BayStaff", bayStaff.Id));
        }
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var bayStaffs = _bayStaffRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var bayStaff in bayStaffs)
        {
            _logger.LogDebug("Handling Step ({Step})\n for this BayStaff \n({@BayStaff})", _modelState.ModelTime, bayStaff);
            
            await StepAsync(bayStaff, cancellationToken);
            
            _logger.LogDebug("Completed handling Step ({Step})\n for this BayStaff \n({@BayStaff})", _modelState.ModelTime, bayStaff);
        }
    }
}