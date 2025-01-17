using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;
using Settings;

namespace Services;

public sealed class StufferService
{
    private readonly ILogger<StufferService> _logger;
    private readonly WorkRepository _workRepository;
    private readonly WorkFactory _workFactory;
    private readonly PalletRepository _palletRepository;
    private readonly BayRepository _bayRepository;
    private readonly HubRepository _hubRepository;
    private readonly PalletService _palletService;
    private readonly AppointmentSlotRepository _appointmentSlotRepository;
    private readonly ModelState _modelState;
    private readonly Instrumentation _instrumentation;

    public StufferService(ILogger<StufferService> logger,
        WorkRepository workRepository,
        WorkFactory workFactory,
        PalletRepository palletRepository,
        BayRepository bayRepository,
        HubRepository hubRepository,
        PalletService palletService,
        AppointmentSlotRepository appointmentSlotRepository,
        ModelState modelState,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _workRepository = workRepository;
        _workFactory = workFactory;
        _palletRepository = palletRepository;
        _bayRepository = bayRepository;
        _hubRepository = hubRepository;
        _palletService = palletService;
        _appointmentSlotRepository = appointmentSlotRepository;
        _modelState = modelState;
        _instrumentation = instrumentation; 
    }

    public async Task AlertWorkCompleteAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var work = await _workRepository.GetAsync(stuffer, cancellationToken);
        if (work == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have Work assigned to alert completed for.", stuffer);

            return;
        }

        var pallet = await _palletRepository.GetAsync(work, cancellationToken);
        if (pallet == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a Pallet assigned to Stuff.", stuffer, work);

            return;
        }
        
        var bay = await _bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a bay assigned to Stuff the Pallet \n({@Pallet})\n for.", stuffer, work, pallet);

            return;
        }
        
        await _palletService.AlertStuffedAsync(pallet, bay, cancellationToken);
        
        _instrumentation.Add(Metric.StufferOccupied, -1, 
        ("Stuffer", stuffer.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));
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
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.", stuffer);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var appointmentSlots = _appointmentSlotRepository.GetAfter(hub, _modelState.ModelTime -
                _modelState.AppointmentConfig!.AppointmentLength * _modelState.ModelConfig.ModelStep)
            .Where(aps => aps.Appointments.Count != 0)
            .OrderBy(aps => aps.StartTime)
            .Take((_modelState.AppointmentConfig!.AppointmentLength / _modelState.AppointmentConfig!.AppointmentSlotDifference) + 1);
        
        Bay? bestBay = null;
        var stuffPalletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPalletCount = (await _palletService
                    .GetAvailableStuffPalletsAsync(bay, appointmentSlots, cancellationToken))
                .Count;
            
            if (bestBay != null && bayStuffPalletCount <= stuffPalletCount)
            {
                continue;
            }
            
            stuffPalletCount = bayStuffPalletCount;
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
            _logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.", stuffer);

            return;
        }

        var bays = _bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var stuffPalletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPalletCount = (await _palletService
                    .GetAvailableStuffPalletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayStuffPalletCount <= stuffPalletCount)
            {
                continue;
            }
            
            stuffPalletCount = bayStuffPalletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            _logger.LogInformation("Stuffer \n({@Stuffer})\n its assigned Hub \n({@Hub})\n did not have a Bay with more Pallets assigned to Stuff.", stuffer, hub);
            
            _logger.LogDebug("Stuffer \n({@Stuffer})\n will remain idle...", stuffer);

            return;
        }

        await StartStuffAsync(stuffer, bestBay, cancellationToken);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, CancellationToken cancellationToken)
    {
        var pallet = await _palletService.GetNextStuffAsync(bay, cancellationToken);
        if (pallet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pallets assigned to Stuff.", bay);
            
            _logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).", bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pallet \n({@Pallet})", stuffer, bay, pallet);
        await _workFactory.GetNewObjectAsync(bay, stuffer, pallet, cancellationToken);
        
        _instrumentation.Add(Metric.StufferOccupied, 1, 
        ("Stuffer", stuffer.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        var pallet = await _palletService.GetNextStuffAsync(bay, appointmentSlots, cancellationToken);
        if (pallet == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have any more Pallets assigned to Stuff.", bay);
            
            _logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).", bay);
            
            return;
        }
        
        _logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pallet \n({@Pallet})", stuffer, bay, pallet);
        await _workFactory.GetNewObjectAsync(bay, stuffer, pallet, cancellationToken);
        
        _instrumentation.Add(Metric.StufferOccupied, 1, 
        ("Stuffer", stuffer.Id),
                ("Bay", bay.Id),
                ("Pallet", pallet.Id));
    }

}