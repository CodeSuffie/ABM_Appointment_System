using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;
using Settings;

namespace Services;

public sealed class PickerService
{
    private readonly ILogger<PickerService> _logger;
    private readonly HubRepository _hubRepository;
    private readonly PalletRepository _palletRepository;
    private readonly PalletService _palletService;
    private readonly BayRepository _bayRepository;
    private readonly BayService _bayService;
    private readonly WorkRepository _workRepository;
    private readonly AppointmentSlotRepository _appointmentSlotRepository;
    private readonly AppointmentRepository _appointmentRepository;
    private readonly WorkFactory _workFactory;
    private readonly ModelState _modelState;
    private readonly Instrumentation _instrumentation;

    public PickerService(ILogger<PickerService> logger,
        HubRepository hubRepository,
        PalletRepository palletRepository,
        PalletService palletService,
        BayRepository bayRepository,
        BayService bayService,
        WorkRepository workRepository,
        AppointmentSlotRepository appointmentSlotRepository,
        AppointmentRepository appointmentRepository,
        WorkFactory workFactory,
        ModelState modelState, 
        Instrumentation instrumentation)
    {
        _logger = logger;
        _hubRepository = hubRepository;
        _palletRepository = palletRepository;
        _palletService = palletService;
        _bayRepository = bayRepository;
        _bayService = bayService;
        _workRepository = workRepository;
        _appointmentSlotRepository = appointmentSlotRepository;
        _appointmentRepository = appointmentRepository;
        _workFactory = workFactory;
        _modelState = modelState;
        _instrumentation = instrumentation;
    }
    
    public async Task AlertWorkCompleteAsync(Picker picker, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(picker, cancellationToken);
        if (work == null)
        {
            _logger.LogError("Picker \n({@Picker})\n did not have Work assigned to alert completed for.", picker);

            return;
        }

        var pallet = await _palletRepository.GetAsync(work, cancellationToken);
        if (pallet == null)
        {
            _logger.LogError("Picker \n({@Picker})\n its assigned Work \n({@Work})\n did not have a Pallet assigned to Fetch.", picker, work);

            return;
        }
        
        var bay = await _bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Picker \n({@Picker})\n its assigned Work \n({@Work})\n did not have a bay assigned to Fetch the Pallet \n({@Pallet})\n for.", picker, work, pallet);

            return;
        }
        
        await _palletService.AlertFetchedAsync(pallet, bay, cancellationToken);
        
        _instrumentation.Add(Metric.PickerOccupied, -1, 
        ("Picker", picker.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));
    }

    public async Task AlertFreeAppointmentAsync(Picker picker, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return;
        }
        
        var hub = await _hubRepository.GetAsync(picker, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to alert free for.", picker);

            return;
        }

        var appointmentSlots = _appointmentSlotRepository.GetAfter(hub, _modelState.ModelTime -
                _modelState.AppointmentConfig!.AppointmentLength * _modelState.ModelConfig.ModelStep)
            .Where(aps => aps.Appointments.Count != 0)
            .OrderBy(aps => aps.StartTime)
            .Take((_modelState.AppointmentConfig!.AppointmentLength / _modelState.AppointmentConfig!.AppointmentSlotDifference) + 1);

        foreach (var appointmentSlot in appointmentSlots)
        {
            var appointments = _appointmentRepository.Get(appointmentSlot)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);


            Bay? bestBay = null;
            Appointment? bestAppointment = null;
            var fetchPalletCount = 0;
            await foreach (var appointment in appointments)
            {
                var bay = await _bayRepository.GetAsync(appointment, cancellationToken);
                if (bay == null)
                {
                    _logger.LogError("Appointment \n({@Appointment})\n did not have a Bay assigned.", appointment);

                    continue;
                }
                
                if (! await _bayService.HasRoomForPalletAsync(bay, cancellationToken))
                {
                    continue;
                }

                var bayFetchPalletCount = (await _palletService
                        .GetAvailableFetchPalletsAsync(bay, appointment, cancellationToken))
                    .Count;
                
                if (bestBay != null && bayFetchPalletCount <= fetchPalletCount)
                {
                    continue;
                }
        
                fetchPalletCount = bayFetchPalletCount;
                bestBay = bay;
                bestAppointment = appointment;
            }

            if (bestBay == null || bestAppointment == null) continue;
            
            await StartFetchAsync(picker, bestBay, bestAppointment, cancellationToken);
            break;
        }
    }

    public async Task AlertFreeAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await _hubRepository.GetAsync(picker, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to alert free for.", picker);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .Where(b => b.BayStatus == BayStatus.Opened &&
                        b.Trip != null &&
                        !b.BayFlags.HasFlag(BayFlags.Fetched))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var fetchPalletCount = 0;
        await foreach (var bay in bays)
        {
            if (! await _bayService.HasRoomForPalletAsync(bay, cancellationToken))
            {
                continue;
            }
            
            var bayFetchPalletCount = (await _palletService
                    .GetAvailableFetchPalletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayFetchPalletCount <= fetchPalletCount)
            {
                continue;
            }
            
            fetchPalletCount = bayFetchPalletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            _logger.LogInformation("Picker \n({@Picker})\n its assigned Hub \n({@Hub})\n did not have a Bay with more Pallets assigned to Fetch.", picker, hub);

            if (!_modelState.ModelConfig.AppointmentSystemMode)
            {
                _logger.LogDebug("Picker \n({@Picker})\n will remain idle...", picker);
            }
            else
            {
                _logger.LogDebug("Picker \n({@Picker})\n will try to fetch for next appointments...", picker);
                
                await AlertFreeAppointmentAsync(picker, cancellationToken);
            }

            return;
        }

        await StartFetchAsync(picker, bestBay, cancellationToken);
    }
    
    private async Task StartFetchAsync(Picker picker, Bay bay, CancellationToken cancellationToken)
    {
        var pallet = await _palletService.GetNextFetchAsync(bay, cancellationToken);
        if (pallet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pallets assigned to Fetch.", bay);
            
            _logger.LogInformation("Fetch Work could not be started for this Bay \n({@Bay}).", bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Picker \n({@Picker})\n at this Bay \n({@Bay}) to Fetch this Pallet \n({@Pallet})", picker, bay, pallet);
        await _workFactory.GetNewObjectAsync(bay, picker, pallet, cancellationToken);
        
        _instrumentation.Add(Metric.PickerOccupied, 1, 
        ("Picker", picker.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));
        
        _instrumentation.Add(Metric.FetchMiss, 1, 
        ("Picker", picker.Id),
            ("Bay", bay.Id),
            ("Pallet", pallet.Id));
    }
    
    private async Task StartFetchAsync(Picker picker, Bay bay, Appointment appointment, CancellationToken cancellationToken)
    {
        var pallet = await _palletService.GetNextFetchAsync(bay, appointment, cancellationToken);
        if (pallet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pallets assigned to Fetch.", bay);
            
            _logger.LogInformation("Fetch Work could not be started for this Bay \n({@Bay}).", bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Picker \n({@Picker})\n at this Bay \n({@Bay}) to Fetch this Pallet \n({@Pallet})", picker, bay, pallet);
        await _workFactory.GetNewObjectAsync(bay, picker, pallet, cancellationToken);
        
        _instrumentation.Add(Metric.PickerOccupied, 1, 
        ("Picker", picker.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));

        var appointmentSlot = await _appointmentSlotRepository.GetAsync(appointment, cancellationToken);
        if (appointmentSlot == null)
        {
            _logger.LogError("Appointment \n({@Appointment})\n did not have an AppointmentSlot assigned to pick for.", picker);

            return;
        }

        if (appointmentSlot.StartTime <= _modelState.ModelTime)
        {
            _instrumentation.Add(Metric.FetchMiss, 1, 
            ("Picker", picker.Id),
                    ("Bay", bay.Id),
                    ("Pallet", pallet.Id));
        }
    }
}