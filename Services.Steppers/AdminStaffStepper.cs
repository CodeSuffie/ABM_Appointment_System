using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Steppers;

public sealed class AdminStaffStepper : IStepperService<AdminStaff>
{
    private readonly ILogger<AdminStaffStepper> _logger;
    private readonly AdminStaffService _adminStaffService;
    private readonly WorkRepository _workRepository;
    private readonly WorkService _workService;
    private readonly HubShiftService _hubShiftService;
    private readonly AdminStaffRepository _adminStaffRepository;
    private readonly ModelState _modelState;
    private readonly Histogram<int> _workingAdminStaffHistogram;
    private readonly Histogram<int> _occupiedAdminStaffHistogram;

    public AdminStaffStepper(
        ILogger<AdminStaffStepper> logger,
        AdminStaffService adminStaffService,
        WorkRepository workRepository,
        WorkService workService,
        HubShiftService hubShiftService,
        AdminStaffRepository adminStaffRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _adminStaffService = adminStaffService;
        _workRepository = workRepository;
        _workService = workService;
        _hubShiftService = hubShiftService;
        _adminStaffRepository = adminStaffRepository;
        _modelState = modelState;

        _workingAdminStaffHistogram = meter.CreateHistogram<int>("working-admin-staff", "AdminStaff", "#AdminStaff Working.");
        _occupiedAdminStaffHistogram = meter.CreateHistogram<int>("occupied-admin-staff", "AdminStaff", "#AdminStaff Occupied.");
    }

    public async Task DataCollectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling Data Collection for AdminStaff in this Step \n({Step})",
            _modelState.ModelTime);
        
        var working = await _adminStaffRepository.CountAsync(_modelState.ModelTime, cancellationToken);
        _workingAdminStaffHistogram.Record(working, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        var occupied = await _adminStaffRepository.CountOccupiedAsync(cancellationToken);
        _occupiedAdminStaffHistogram.Record(occupied, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        
        _logger.LogDebug("Finished handling Data Collection for AdminStaff in this Step \n({Step})",
            _modelState.ModelTime);
    }
    
    public async Task StepAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(adminStaff, cancellationToken);
        
        if (work == null)
        {
            _logger.LogInformation("AdminStaff \n({@AdminStaff})\n does not have active Work assigned in this Step \n({Step})",
                             adminStaff,
                             _modelState.ModelTime);
            
            var shift = await _hubShiftService.GetCurrentAsync(adminStaff, cancellationToken);
            if (shift == null)
            {
                _logger.LogInformation("AdminStaff \n({@AdminStaff})\n is not working in this Step \n({Step})",
                    adminStaff,
                    _modelState.ModelTime);
                
                _logger.LogDebug("AdminStaff \n({@AdminStaff})\n will remain idle in this Step \n({Step})",
                    adminStaff,
                    _modelState.ModelTime);
                
                return;
            }
            
            _logger.LogDebug("Alerting Free for this AdminStaff \n({@AdminStaff})\n in this Step \n({Step})",
                adminStaff,
                _modelState.ModelTime);
            await _adminStaffService.AlertFreeAsync(adminStaff, cancellationToken);
            
            return;
        }
        
        if (_workService.IsWorkCompleted(work))
        {
            _logger.LogInformation("AdminStaff \n({@AdminStaff})\n just completed assigned Work \n({@Work})\n in this Step \n({Step})",
                adminStaff,
                work,
                _modelState.ModelTime);
            
            _logger.LogDebug("Alerting Work Completed for this AdminStaff \n({@AdminStaff})\n in this Step \n({Step})",
                adminStaff,
                _modelState.ModelTime);
            await _adminStaffService.AlertWorkCompleteAsync(adminStaff, cancellationToken);
        }
    }
    
    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var adminStaffs = _adminStaffRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var adminStaff in adminStaffs)
        {
            _logger.LogDebug("Handling Step \n({Step})\n for this AdminStaff \n({@AdminStaff})",
                _modelState.ModelTime,
                adminStaff);
            
            await StepAsync(adminStaff, cancellationToken);
            
            _logger.LogDebug("Completed handling Step \n({Step})\n for this AdminStaff \n({@AdminStaff})",
                _modelState.ModelTime,
                adminStaff);
        }
    }
}