using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services;

public sealed class PickerService
{
    private readonly ILogger<PickerService> _logger;
    private readonly HubRepository _hubRepository;
    private readonly PelletRepository _pelletRepository;
    private readonly PelletService _pelletService;
    private readonly BayRepository _bayRepository;
    private readonly BayService _bayService;
    private readonly WorkRepository _workRepository;
    private readonly AppointmentSlotRepository _appointmentSlotRepository;
    private readonly AppointmentRepository _appointmentRepository;
    private readonly WorkFactory _workFactory;
    private readonly ModelState _modelState;
    private readonly Counter<int> _fetchMissCounter;

    public PickerService(ILogger<PickerService> logger,
        HubRepository hubRepository,
        PelletRepository pelletRepository,
        PelletService pelletService,
        BayRepository bayRepository,
        BayService bayService,
        WorkRepository workRepository,
        AppointmentSlotRepository appointmentSlotRepository,
        AppointmentRepository appointmentRepository,
        WorkFactory workFactory,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _hubRepository = hubRepository;
        _pelletRepository = pelletRepository;
        _pelletService = pelletService;
        _bayRepository = bayRepository;
        _bayService = bayService;
        _workRepository = workRepository;
        _appointmentSlotRepository = appointmentSlotRepository;
        _appointmentRepository = appointmentRepository;
        _workFactory = workFactory;
        _modelState = modelState;
        
        _fetchMissCounter = meter.CreateCounter<int>("fetch-miss", "FetchMiss", "#PickUp Load not fetched yet.");
    }
    
    public async Task AlertWorkCompleteAsync(Picker picker, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(picker, cancellationToken);
        if (work == null)
        {
            _logger.LogError("Picker \n({@Picker})\n did not have Work assigned to alert completed for.",
                picker);

            return;
        }

        var pellet = await _pelletRepository.GetAsync(work, cancellationToken);
        if (pellet == null)
        {
            _logger.LogError("Picker \n({@Picker})\n its assigned Work \n({@Work})\n did not have a Pellet assigned to Fetch.",
                picker,
                work);

            return;
        }
        
        var bay = await _bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Picker \n({@Picker})\n its assigned Work \n({@Work})\n did not have a bay assigned to Fetch the Pellet \n({@Pellet})\n for.",
                picker,
                work,
                pellet);

            return;
        }
        
        await _pelletService.AlertFetchedAsync(pellet, bay, cancellationToken);
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
            _logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to alert free for.",
                picker);

            return;
        }

        var appointmentSlots = _appointmentSlotRepository.GetAfter(hub,
                _modelState.ModelTime -
                _modelState.AppointmentConfig!.AppointmentLength * _modelState.ModelConfig.ModelStep)
            .Where(aps => aps.Appointments.Count != 0)
            .OrderBy(aps => aps.StartTime)
            .Take(2);

        foreach (var appointmentSlot in appointmentSlots)
        {
            var appointments = _appointmentRepository.Get(appointmentSlot)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);


            Bay? bestBay = null;
            Appointment? bestAppointment = null;
            var fetchPelletCount = 0;
            await foreach (var appointment in appointments)
            {
                var bay = await _bayRepository.GetAsync(appointment, cancellationToken);
                if (bay == null)
                {
                    _logger.LogError("Appointment \n({@Appointment})\n did not have a Bay assigned.",
                        appointment);

                    continue;
                }
                
                if (! await _bayService.HasRoomForPelletAsync(bay, cancellationToken))
                {
                    continue;
                }

                var bayFetchPelletCount = (await _pelletService
                        .GetAvailableFetchPelletsAsync(bay, appointment, cancellationToken))
                    .Count;
                
                if (bestBay != null && bayFetchPelletCount <= fetchPelletCount)
                {
                    continue;
                }
        
                fetchPelletCount = bayFetchPelletCount;
                bestBay = bay;
                bestAppointment = appointment;
            }
            
            if (bestBay != null && bestAppointment != null)
            {
                await StartFetchAsync(picker, bestBay, bestAppointment, cancellationToken);
            }
        }
    }

    public async Task AlertFreeAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await _hubRepository.GetAsync(picker, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to alert free for.",
                picker);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .Where(b => b.BayStatus == BayStatus.Opened &&
                        b.Trip != null &&
                        !b.BayFlags.HasFlag(BayFlags.Fetched))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var fetchPelletCount = 0;
        await foreach (var bay in bays)
        {
            if (! await _bayService.HasRoomForPelletAsync(bay, cancellationToken))
            {
                continue;
            }
            
            var bayFetchPelletCount = (await _pelletService
                    .GetAvailableFetchPelletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayFetchPelletCount <= fetchPelletCount)
            {
                continue;
            }
            
            fetchPelletCount = bayFetchPelletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            _logger.LogInformation("Picker \n({@Picker})\n its assigned Hub \n({@Hub})\n did not have a " +
                                   "Bay with more Pellets assigned to Fetch.",
                picker,
                hub);

            if (!_modelState.ModelConfig.AppointmentSystemMode)
            {
                _logger.LogDebug("Picker \n({@Picker})\n will remain idle...",
                    picker);
            }
            else
            {
                _logger.LogDebug("Picker \n({@Picker})\n will try to fetch for next appointments...",
                    picker);
                
                await AlertFreeAppointmentAsync(picker, cancellationToken);
            }

            return;
        }

        await StartFetchAsync(picker, bestBay, cancellationToken);
    }
    
    private async Task StartFetchAsync(Picker picker, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextFetchAsync(bay, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Fetch.",
                bay);
            
            _logger.LogInformation("Fetch Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Picker \n({@Picker})\n at this Bay \n({@Bay}) to Fetch this Pellet \n({@Pellet})",
            picker,
            bay,
            pellet);
        await _workFactory.GetNewObjectAsync(bay, picker, pellet, cancellationToken);
        
        _fetchMissCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
    }
    
    private async Task StartFetchAsync(Picker picker, Bay bay, Appointment appointment, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextFetchAsync(bay, appointment, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Fetch.",
                bay);
            
            _logger.LogInformation("Fetch Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Picker \n({@Picker})\n at this Bay \n({@Bay}) to Fetch this Pellet \n({@Pellet})",
            picker,
            bay,
            pellet);
        await _workFactory.GetNewObjectAsync(bay, picker, pellet, cancellationToken);

        var appointmentSlot = await _appointmentSlotRepository.GetAsync(appointment, cancellationToken);
        if (appointmentSlot == null)
        {
            _logger.LogError("Appointment \n({@Appointment})\n did not have an AppointmentSlot assigned to pick for.",
                picker);

            return;
        }

        if (appointmentSlot.StartTime <= _modelState.ModelTime)
        {
            _fetchMissCounter.Add(1, new KeyValuePair<string, object?>("Step", _modelState.ModelTime));
        }
    }
}