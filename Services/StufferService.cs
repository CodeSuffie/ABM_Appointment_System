using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services;

public sealed class StufferService
{
    private readonly ILogger<StufferService> _logger;
    private readonly WorkRepository _workRepository;
    private readonly WorkFactory _workFactory;
    private readonly PelletRepository _pelletRepository;
    private readonly BayRepository _bayRepository;
    private readonly HubRepository _hubRepository;
    private readonly PelletService _pelletService;
    private readonly AppointmentSlotRepository _appointmentSlotRepository;
    private readonly ModelState _modelState;
        
    private readonly UpDownCounter<int> _occupiedStufferCounter;

    public StufferService(ILogger<StufferService> logger,
        WorkRepository workRepository,
        WorkFactory workFactory,
        PelletRepository pelletRepository,
        BayRepository bayRepository,
        HubRepository hubRepository,
        PelletService pelletService,
        AppointmentSlotRepository appointmentSlotRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _workRepository = workRepository;
        _workFactory = workFactory;
        _pelletRepository = pelletRepository;
        _bayRepository = bayRepository;
        _hubRepository = hubRepository;
        _pelletService = pelletService;
        _appointmentSlotRepository = appointmentSlotRepository;
        _modelState = modelState;
        
        _occupiedStufferCounter = meter.CreateUpDownCounter<int>("fetch-stuffer", "Stuffer", "#Stuffer Working on a Stuff.");
    }

    public async Task AlertWorkCompleteAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(stuffer, cancellationToken);
        if (work == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have Work assigned to alert completed for.",
                stuffer);

            return;
        }

        var pellet = await _pelletRepository.GetAsync(work, cancellationToken);
        if (pellet == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a Pellet assigned to Stuff.",
                stuffer,
                work);

            return;
        }
        
        var bay = await _bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a bay assigned to Stuff the Pellet \n({@Pellet})\n for.",
                stuffer,
                work,
                pellet);

            return;
        }
        
        await _pelletService.AlertStuffedAsync(pellet, bay, cancellationToken);
        
        _occupiedStufferCounter.Add(-1, 
        [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("Stuffer", stuffer.Id),
                new KeyValuePair<string, object?>("Bay", bay.Id),
                new KeyValuePair<string, object?>("Pellet", pellet.Id),
            ]);
    }
    
    public async Task AlertFreeAppointmentAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return;
        }
        
        var hub = await _hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.",
                stuffer);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var appointmentSlots = _appointmentSlotRepository.GetAfter(hub,
                _modelState.ModelTime -
                _modelState.AppointmentConfig!.AppointmentLength * _modelState.ModelConfig.ModelStep)
            .Where(aps => aps.Appointments.Count != 0)
            .OrderBy(aps => aps.StartTime)
            .Take((_modelState.AppointmentConfig!.AppointmentLength / _modelState.AppointmentConfig!.AppointmentSlotDifference) + 1);
        
        Bay? bestBay = null;
        var stuffPelletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPelletCount = (await _pelletService
                    .GetAvailableStuffPelletsAsync(bay, appointmentSlots, cancellationToken))
                .Count;
            
            if (bestBay != null && bayStuffPelletCount <= stuffPelletCount)
            {
                continue;
            }
            
            stuffPelletCount = bayStuffPelletCount;
            bestBay = bay;
        }

        if (bestBay != null)
        {
            await StartStuffAsync(stuffer, bestBay, appointmentSlots, cancellationToken);
        }
    }
    public async Task AlertFreeAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        if (_modelState.ModelConfig.AppointmentSystemMode)
        {
            await AlertFreeAppointmentAsync(stuffer, cancellationToken);
            
            return;
        }
        
        var hub = await _hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.",
                stuffer);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var stuffPelletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPelletCount = (await _pelletService
                    .GetAvailableStuffPelletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayStuffPelletCount <= stuffPelletCount)
            {
                continue;
            }
            
            stuffPelletCount = bayStuffPelletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            _logger.LogInformation("Stuffer \n({@Stuffer})\n its assigned Hub \n({@Hub})\n did not have a " +
                                   "Bay with more Pellets assigned to Stuff.",
                stuffer,
                hub);
            
            _logger.LogDebug("Stuffer \n({@Stuffer})\n will remain idle...",
                stuffer);

            return;
        }

        await StartStuffAsync(stuffer, bestBay, cancellationToken);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextStuffAsync(bay, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Stuff.",
                bay);
            
            _logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pellet \n({@Pellet})",
            stuffer,
            bay,
            pellet);
        await _workFactory.GetNewObjectAsync(bay, stuffer, pellet, cancellationToken);
        
        _occupiedStufferCounter.Add(1, 
        [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("Stuffer", stuffer.Id),
                new KeyValuePair<string, object?>("Bay", bay.Id),
                new KeyValuePair<string, object?>("Pellet", pellet.Id)
            ]);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        var pellet = await _pelletService.GetNextStuffAsync(bay, appointmentSlots, cancellationToken);
        if (pellet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Stuff.",
                bay);
            
            _logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pellet \n({@Pellet})",
            stuffer,
            bay,
            pellet);
        await _workFactory.GetNewObjectAsync(bay, stuffer, pellet, cancellationToken);
        
        _occupiedStufferCounter.Add(1, 
        [
                new KeyValuePair<string, object?>("Step", _modelState.ModelTime),
                new KeyValuePair<string, object?>("Stuffer", stuffer.Id),
                new KeyValuePair<string, object?>("Bay", bay.Id),
                new KeyValuePair<string, object?>("Pellet", pellet.Id),
                // new KeyValuePair<string, object?>("Appointments", appointmentSlots.Select(aps => aps.Id).ToList())
            ]);
    }

}