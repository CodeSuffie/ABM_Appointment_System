using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

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
    private readonly Histogram<int> _workingBayStaffHistogram;
    private readonly Histogram<int> _dropOffBayStaffHistogram;
    private readonly Histogram<int> _pickUpBayStaffHistogram;

    public BayStaffStepper(
        ILogger<BayStaffStepper> logger,
        BayRepository bayRepository,
        BayShiftService bayShiftService,
        WorkRepository workRepository,
        WorkService workService,
        BayStaffService bayStaffService,
        BayStaffRepository bayStaffRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _bayRepository = bayRepository;
        _bayShiftService = bayShiftService;
        _workRepository = workRepository;
        _workService = workService;
        _bayStaffService = bayStaffService;
        _bayStaffRepository = bayStaffRepository;
        _modelState = modelState;

        _workingBayStaffHistogram = meter.CreateHistogram<int>("working-bay-staff", "BayStaff", "#BayStaff Working.");
        _dropOffBayStaffHistogram = meter.CreateHistogram<int>("drop-off-bay-staff", "BayStaff", "#BayStaff Working on a Drop-Off.");
        _pickUpBayStaffHistogram = meter.CreateHistogram<int>("pick-up-bay-staff", "BayStaff", "#BayStaff Working on a Pick-Up.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for BayStaff in this Step ({Step})", _modelState.ModelTime);
        
        // var working = await _bayStaffRepository.CountAsync(_modelState.ModelTime, cancellationToken);
        // _workingBayStaffHistogram.Record(working, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var dropOff = await _bayStaffRepository.CountAsync(WorkType.DropOff, cancellationToken);
        // _dropOffBayStaffHistogram.Record(dropOff, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        //
        // var pickUp = await _bayStaffRepository.CountAsync(WorkType.PickUp, cancellationToken);
        // _pickUpBayStaffHistogram.Record(pickUp, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for BayStaff in this Step ({Step})", _modelState.ModelTime);
    }
    
    public async Task StepAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(bayStaff, cancellationToken);
        
        if (work == null)
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n does not have active Work assigned in this Step ({Step})", bayStaff, _modelState.ModelTime);
            
            var shift = await _bayShiftService.GetCurrentAsync(bayStaff, cancellationToken);
            if (shift == null)
            {
                _logger.LogInformation("BayStaff \n({@BayStaff})\n is not working in this Step ({Step})", bayStaff, _modelState.ModelTime);
                
                _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);
                
                return;
            }
            
            var bay = await _bayRepository.GetAsync(shift, cancellationToken);
            if (bay == null)
            {
                _logger.LogError("The current BayShift \n({@BayShift})\n for this BayStaff \n({@BayStaff})\n did not have a Bay assigned.", shift, bayStaff);
                
                _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);
                
                return;
            }
            
            _logger.LogDebug("Alerting Free for this BayStaff \n({@BayStaff})\n in this Step ({Step})", bayStaff, _modelState.ModelTime);
            await _bayStaffService.AlertFreeAsync(bayStaff, bay, cancellationToken);
            
            return;
        }
        
        if (_workService.IsWorkCompleted(work))
        {
            _logger.LogInformation("BayStaff \n({@BayStaff})\n just completed assigned Work \n({@Work})\n in this Step ({Step})", bayStaff, work, _modelState.ModelTime);
            
            var bay = await _bayRepository.GetAsync(work, cancellationToken);
            if (bay == null)
            {
                _logger.LogError("The active assigned Work \n({@Work})\n for this BayStaff \n({@BayStaff})\n did not have a Bay assigned.", work, bayStaff);

                //_logger.LogDebug("Removing invalid Work \n({@Work})\n for this BayStaff \n({@BayStaff})",
                //    work,
                //    bayStaff);
                //await _workRepository.RemoveAsync(work, cancellationToken);
                
                _logger.LogDebug("BayStaff \n({@BayStaff})\n will remain idle in this Step ({Step})", bayStaff, _modelState.ModelTime);

                return;
            }

            _logger.LogDebug("Alerting Work Completed for this BayStaff \n({@BayStaff})\n in this Step ({Step})", bayStaff, _modelState.ModelTime);
            await _bayStaffService.AlertWorkCompleteAsync(work, bay, bayStaff, cancellationToken);
            
            _logger.LogDebug("Removing old Work \n({@Work})\n for this BayStaff \n({@BayStaff})", work, bayStaff);
            await _workRepository.RemoveAsync(work, cancellationToken);
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