using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services;

public sealed class PalletService
{
    private readonly ILogger<PalletService> _logger;
    private readonly ModelState _modelState;
    private readonly PalletRepository _palletRepository;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly TruckRepository _truckRepository;
    private readonly HubRepository _hubRepository;
    private readonly WarehouseRepository _warehouseRepository;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly WorkRepository _workRepository;
    private readonly LoadRepository _loadRepository;
    private readonly AppointmentRepository _appointmentRepository;
    private readonly PickerRepository _pickerRepository;
    private readonly BayStaffRepository _bayStaffRepository;

    public PalletService(
        ILogger<PalletService> logger,
        ModelState modelState,
        PalletRepository palletRepository,
        TruckCompanyRepository truckCompanyRepository,
        TruckRepository truckRepository,
        HubRepository hubRepository,
        WarehouseRepository warehouseRepository,
        BayRepository bayRepository,
        TripRepository tripRepository,
        WorkRepository workRepository,
        LoadRepository loadRepository,
        AppointmentRepository appointmentRepository,
        PickerRepository pickerRepository,
        BayStaffRepository bayStaffRepository)
    {
        _logger = logger;
        _palletRepository = palletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _truckRepository = truckRepository;
        _hubRepository = hubRepository;
        _warehouseRepository = warehouseRepository;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _loadRepository = loadRepository;
        _appointmentRepository = appointmentRepository;
        _pickerRepository = pickerRepository;
        _bayStaffRepository = bayStaffRepository;
        _modelState = modelState;
    }

    private async Task<bool> HasPalletAsync(Bay bay, Pallet pallet, CancellationToken cancellationToken)
    {
        return (await _palletRepository
                   .Get(bay)
                   .FirstOrDefaultAsync(p => p.Id == pallet.Id,
                       cancellationToken))
               != null;
    }
    
    private async Task<bool> HasPalletAsync(IQueryable<Pallet>? pallets, Pallet pallet, CancellationToken cancellationToken)
    {
        return pallets != null && await pallets.AnyAsync(p => p.Id == pallet.Id,
            cancellationToken);
    }
    
    private async Task<bool> HasWorkAsync(Pallet pallet, CancellationToken cancellationToken)
    {
        return (await _workRepository
            .GetAsync(pallet, cancellationToken))
               != null;
    }

    private async Task DropOff(Pallet pallet, Truck truck, Bay bay, CancellationToken cancellationToken)
    {
        await _palletRepository.UnsetAsync(pallet, truck, cancellationToken);
        await _palletRepository.SetAsync(pallet, bay, cancellationToken);
    }
    
    private async Task Stuff(Pallet pallet, Bay bay, Warehouse warehouse, CancellationToken cancellationToken)
    {
        await _palletRepository.UnsetAsync(pallet, bay, cancellationToken);
        await _palletRepository.SetAsync(pallet, warehouse, cancellationToken);
    }
    
    private async Task Fetch(Pallet pallet, Warehouse warehouse, Bay bay, CancellationToken cancellationToken)
    {
        await _palletRepository.UnsetAsync(pallet, warehouse, cancellationToken);
        await _palletRepository.SetAsync(pallet, bay, cancellationToken);
    }
    
    private async Task PickUp(Pallet pallet, Bay bay, Truck truck, CancellationToken cancellationToken)
    {
        await _palletRepository.UnsetAsync(pallet, bay, cancellationToken);
        await _palletRepository.SetAsync(pallet, truck, cancellationToken);
    }

    public async Task AlertDroppedOffAsync(Pallet pallet, Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned.", bay);
            
            return;
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned to alert Dropped Off for.", trip);
            
            return;
        }

        if (_palletRepository.Get(truck).FirstOrDefault(p => p.Id == pallet.Id) == null)
        {
            _logger.LogError("Cannot unload Pallet \n({@Pallet})\n for this Trip \n({@Trip})\n from this Truck \n({@Truck})\n at this Bay \n({@Bay})\n since its Inventory does not have the Pallet assigned.", pallet, trip, bay);

            return;
        }

        await DropOff(pallet, truck, bay, cancellationToken);
    }
    
    public async Task AlertStuffedAsync(Pallet pallet, Bay bay, CancellationToken cancellationToken)
    {
        var palletBay = await _bayRepository.GetAsync(pallet, cancellationToken);
        if (palletBay == null)
        {
            _logger.LogError("Pallet ({@Pallet}) did not have a Bay assigned.", bay);
            
            return;
        }

        if (palletBay.Id != bay.Id)
        {
            _logger.LogError("Cannot Stuff Pallet ({@Pallet}) from this Bay \n({@Bay})\n because the Pallet had a different Bay \n({@Bay})\n assigned.", pallet, bay, palletBay);
            
            return;
        }
        
        var hub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Bay ({@Bay}) did not have a Hub assigned.", bay);
            
            return;
        }

        var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
        if (warehouse == null)
        {
            _logger.LogError("Hub ({@Hub}) did not have a Warehouse assigned.", hub);
            
            return;
        }

        await Stuff(pallet, bay, warehouse, cancellationToken);
    }
    
    public async Task AlertFetchedAsync(Pallet pallet, Bay bay, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetAsync(pallet, cancellationToken);
        if (warehouse == null)
        {
            _logger.LogError("Pallet ({@Pallet}) did not have a Warehouse assigned to Fetch from.", pallet);
            
            return;
        }

        var warehouseHub = await _hubRepository.GetAsync(warehouse, cancellationToken);
        if (warehouseHub == null)
        {
            _logger.LogError("Warehouse ({@Warehouse}) did not have a Hub assigned.", warehouse);
            
            return;
        }
        
        var bayHub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (bayHub == null)
        {
            _logger.LogError("Bay ({@Bay}) did not have a Hub assigned.", bay);
            
            return;
        }

        if (warehouseHub.Id != bayHub.Id)
        {
            _logger.LogError("Pallet ({@Pallet}) at this Warehouse ({@Warehouse}) at this Hub ({@Hub}) could not be fetched for this Bay ({@Bay}) because its Hub ({@Hub}) does not have the Pallet Warehouse assigned.", pallet, warehouse, warehouseHub, bay, bayHub);
            
            return;
        }

        await Fetch(pallet, warehouse, bay, cancellationToken);
    }

    public async Task AlertPickedUpAsync(Pallet pallet, Trip trip, CancellationToken cancellationToken)
    {
        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Bay assigned.", trip);
            
            return;
        }

        var bayPallets = _palletRepository.Get(bay);
        if (!bayPallets.Any(p => p.Id == pallet.Id))
        {
            _logger.LogError("Cannot load Pallet ({@Pallet}) for this Trip ({@Trip}) since its Bay ({@Bay}) does not have the pallet assigned.", pallet, trip, bay);
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned to alert Picked Up for.", trip);
            
            return;
        }
        
        if (_palletRepository.Get(truck).FirstOrDefault(p => p.Id == pallet.Id) != null)
        {
            _logger.LogError("Cannot unload Pallet \n({@Pallet})\n for this Trip \n({@Trip})\n onto this Truck \n({@Truck})\n at this Bay \n({@Bay})\n since its Inventory already has the Pallet assigned.", pallet, trip, truck, bay);

            return;
        }
        
        await PickUp(pallet, bay, truck, cancellationToken);
    }
    
    public async Task<List<Pallet>> GetDropOffPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned.", bay);
            
            return [];
        }
        
        var dropOffLoad = await _loadRepository.GetAsync(trip, LoadType.DropOff, cancellationToken);
        if (dropOffLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Drop-Off.", trip);
            
            return [];
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned to get DropOff Pallets for.", trip);
            
            return [];
        }
        
        var dropOffPallets = new List<Pallet>();
        var truckPallets = _palletRepository.Get(truck);

        foreach (var dropOffPallet in dropOffLoad.Pallets)
        {
            if (await HasPalletAsync(truckPallets, dropOffPallet, cancellationToken))
            {
                dropOffPallets.Add(dropOffPallet);
            }
        }

        return dropOffPallets;
    }

    public async Task<List<Pallet>> GetStuffPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var bayPallets = _palletRepository.Get(bay);
        
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned.", bay);
            
            return bayPallets.ToList();
        }
        
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.", trip);
            
            return bayPallets.ToList();
        }
        
        var stuffPallets = new List<Pallet>();

        foreach (var bayPallet in bayPallets)
        {
            if (pickUpLoad.Pallets.All(p => p.Id != bayPallet.Id))
            {
                stuffPallets.Add(bayPallet);
            }
        }

        return stuffPallets;
    }

    public async Task<List<Pallet>> GetStuffPalletsAsync(Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return [];
        }
        
        var stuffPallets = _palletRepository.Get(bay)
            .AsEnumerable();
        
        foreach (var appointmentSlot in appointmentSlots)
        {
            var appointment = await _appointmentRepository.GetAsync(bay, appointmentSlot, cancellationToken);
            if (appointment == null)
            {
                _logger.LogInformation("Bay \n({@Bay})\n did not have an Appointment assigned during this AppointmentSlot \n({@AppointmentSlot}).", bay, appointmentSlot);

                continue;
            }
            
            var trip = await _tripRepository.GetAsync(appointment, cancellationToken);
            if (trip == null)
            {
                _logger.LogError("Appointment \n{@Appointment}\n assigned to this Bay \n({@Bay})\n during this AppointmentSlot \n({@AppointmentSlot}) did not have a Trip assigned.", appointment, bay, appointmentSlot);

                continue;
            }

            var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
            if (pickUpLoad == null)
            {
                _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Pick-Up.", trip);

                continue;
            }

            stuffPallets = stuffPallets
                .Where(stuffPallet => 
                    pickUpLoad.Pallets.All(p => p.Id != stuffPallet.Id));
        }

        var tripCurrent = await _tripRepository.GetAsync(bay, cancellationToken);
        if (tripCurrent == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned.", bay);

            return stuffPallets.ToList();
        }
        
        var pickUpLoadCurrent = await _loadRepository.GetAsync(tripCurrent, LoadType.PickUp, cancellationToken);
        if (pickUpLoadCurrent == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n did not have a Load assigned to Pick-Up.", tripCurrent);

            return stuffPallets.ToList();
        }
        
        stuffPallets = stuffPallets
            .Where(stuffPallet => 
                pickUpLoadCurrent.Pallets.All(p => p.Id != stuffPallet.Id));

        return stuffPallets.ToList();
    }
    
    public async Task<List<Pallet>> GetFetchPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned.", bay);
            
            return [];
        }
        
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n at Bay \n({@Bay})\n did not have a Load assigned to Pick-Up.", trip, bay);
            
            return [];
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned get Fetch Pallets for.", trip);
            
            return [];
        }
        
        var fetchPallets = new List<Pallet>();
        var truckPallets = _palletRepository.Get(truck);

        foreach (var pickUpPallet in pickUpLoad.Pallets)
        {
            if (!await HasPalletAsync(truckPallets, pickUpPallet, cancellationToken) &&
                !await HasPalletAsync(bay, pickUpPallet, cancellationToken))   // TODO: Check if Bay Hub matches Warehouse Hub
            {
                fetchPallets.Add(pickUpPallet);
            }
        }

        return fetchPallets;
    }
    
    public async Task<List<Pallet>> GetFetchPalletsAsync(Bay bay, Appointment appointment, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return [];
        }
        
        var trip = await _tripRepository.GetAsync(appointment, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Appointment \n({@Appointment})\n did not have a Trip assigned.", appointment);
            
            return [];
        }
        
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip \n({@Trip})\n assigned to this Appointment \n({@Appointment})\n at Bay \n({@Bay})\n did not have a Load assigned to Pick-Up.", trip, appointment, bay);
            
            return [];
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        IQueryable<Pallet>? truckPallets = null;
        if (truck != null)
        {
            truckPallets = _palletRepository.Get(truck);
        }
        
        var fetchPallets = new List<Pallet>();

        foreach (var pickUpPallet in pickUpLoad.Pallets)
        {
            if (!await HasPalletAsync(truckPallets, pickUpPallet, cancellationToken) &&
                !await HasPalletAsync(bay, pickUpPallet, cancellationToken))   // TODO: Check if Bay Hub matches Warehouse Hub
            {
                fetchPallets.Add(pickUpPallet);
            }
        }

        return fetchPallets;
    }

    public async Task<List<Pallet>> GetPickUpPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogInformation("Bay \n({@Bay})\n did not have a Trip assigned.", bay);
            
            return [];
        }
        
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.", trip);

            return [];
        }

        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned to get PickUp Pallets for.", trip);
            
            return [];
        }
        
        var pickUpPallets = new List<Pallet>();
        var truckPallets = _palletRepository.Get(truck);

        foreach (var pickUpPallet in pickUpLoad.Pallets)
        {
            if (!await HasPalletAsync(truckPallets, pickUpPallet, cancellationToken))
            {
                pickUpPallets.Add(pickUpPallet);
            }
        }

        return pickUpPallets;
    }

    public async Task<List<Pallet>> GetAvailableDropOffPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetDropOffPalletsAsync(bay, cancellationToken);
        var dropOffPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            if (work == null)
            {
                dropOffPallets.Add(pallet);
            }
        }

        return dropOffPallets;
    }
    
    public async Task<List<Pallet>> GetAvailableStuffPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetStuffPalletsAsync(bay, cancellationToken);
        var stuffPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            if (work == null)
            {
                stuffPallets.Add(pallet);
            }
        }

        return stuffPallets;
    }
    
    public async Task<List<Pallet>> GetAvailableStuffPalletsAsync(Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return [];
        }
        
        var pallets = await GetStuffPalletsAsync(bay, appointmentSlots, cancellationToken);
        var stuffPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            if (work == null)
            {
                stuffPallets.Add(pallet);
            }
        }

        return stuffPallets;
    }

    public async Task<List<Pallet>> GetAvailableFetchPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetFetchPalletsAsync(bay, cancellationToken);
        var fetchPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            // TODO: Work does not get removed from Pallets correctly
            if (work == null)
            {
                fetchPallets.Add(pallet);
            }
            else
            {
                var picker = await _pickerRepository.GetAsync(work, cancellationToken);
                var bayStaff = await _bayStaffRepository.GetAsync(work, cancellationToken);
                if (picker == null && bayStaff == null)
                {
                    await _workRepository.RemoveAsync(work, cancellationToken);
                    fetchPallets.Add(pallet);
                }
            }
        }

        return fetchPallets;
    }
    
    public async Task<List<Pallet>> GetAvailableFetchPalletsAsync(Bay bay, Appointment appointment, CancellationToken cancellationToken)
    {
        if (!_modelState.ModelConfig.AppointmentSystemMode)
        {
            _logger.LogError("This function cannot be called without Appointment System Mode.");

            return [];
        }
        
        var pallets = await GetFetchPalletsAsync(bay, appointment, cancellationToken);
        var fetchPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            if (work == null)
            {
                fetchPallets.Add(pallet);
            }
        }

        return fetchPallets;
    }
    
    public async Task<List<Pallet>> GetAvailablePickUpPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetPickUpPalletsAsync(bay, cancellationToken);

        pallets = pallets
            .Where(p => p.BayId == bay.Id)
            .ToList();
        
        var pickUpPallets = new List<Pallet>();
        
        foreach (var pallet in pallets)
        {
            var work = await _workRepository
                .GetAsync(pallet, cancellationToken);
            if (work == null)
            {
                pickUpPallets.Add(pallet);
            }
        }

        return pickUpPallets;
    }

    public async Task<Pallet?> GetNextDropOffAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailableDropOffPalletsAsync(bay, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }
    
    public async Task<Pallet?> GetNextStuffAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailableStuffPalletsAsync(bay, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }
    
    public async Task<Pallet?> GetNextStuffAsync(Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailableStuffPalletsAsync(bay, appointmentSlots, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }

    public async Task<Pallet?> GetNextFetchAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailableFetchPalletsAsync(bay, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }

    public async Task<Pallet?> GetNextFetchAsync(Bay bay, Appointment appointment, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailableFetchPalletsAsync(bay, appointment, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }
    
    public async Task<Pallet?> GetNextPickUpAsync(Bay bay, CancellationToken cancellationToken)
    {
        var pallets = await GetAvailablePickUpPalletsAsync(bay, cancellationToken);
        return pallets.Count <= 0 ? null : pallets[0];
    }

    public async Task<bool> HasDropOffPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        return (await GetDropOffPalletsAsync(bay, cancellationToken)).Count > 0;
    }

    public async Task<bool> HasFetchPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        return (await GetFetchPalletsAsync(bay, cancellationToken)).Count > 0;
    }

    public async Task<bool> HasPickUpPalletsAsync(Bay bay, CancellationToken cancellationToken)
    {
        return (await GetPickUpPalletsAsync(bay, cancellationToken)).Count > 0;
    }

    public async Task CompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned to alert Complete for.", trip);
            
            return;
        }

        await UnloadPalletsAsync(truck, trip, cancellationToken);
    }

    public async Task LoadPalletsAsync(Truck truck, Load load, CancellationToken cancellationToken)
    {
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) to add the new Inventory for.", truck);

            return;
        }
        
        var pallets = _palletRepository.Get(load)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var pallet in pallets)
        {
            await _palletRepository.UnsetAsync(pallet, truckCompany, cancellationToken);
            await _palletRepository.SetAsync(pallet, truck, cancellationToken);
        }
    }
    
    public async Task UnloadPalletsAsync(Truck truck, Trip trip, CancellationToken cancellationToken)
    {
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) unload the inventory for.", truck);

            return;
        }

        var loads = _loadRepository.Get(trip)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var load in loads)
        {
            var loadPallets = _palletRepository.Get(load)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            await foreach (var loadPallet in loadPallets)
            {
                await _palletRepository.UnsetAsync(loadPallet, load, cancellationToken);
            }
        }
        
        var pallets = _palletRepository.Get(truck)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var pallet in pallets)
        {
            await _palletRepository.UnsetAsync(pallet, truck, cancellationToken);
            await _palletRepository.UnsetAsync(pallet, truck, cancellationToken);
            await _palletRepository.SetAsync(pallet, truckCompany, cancellationToken);
        }
    }
}