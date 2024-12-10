using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services.PelletServices;

public sealed class PelletService
{
    private readonly ILogger<PelletService> _logger;
    private readonly ModelState _modelState;
    private readonly PelletRepository _pelletRepository;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly TruckRepository _truckRepository;
    private readonly HubRepository _hubRepository;
    private readonly WarehouseRepository _warehouseRepository;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly WorkRepository _workRepository;
    private readonly LoadRepository _loadRepository;

    public PelletService(
        ILogger<PelletService> logger,
        ModelState modelState,
        PelletRepository pelletRepository,
        TruckCompanyRepository truckCompanyRepository,
        TruckRepository truckRepository,
        HubRepository hubRepository,
        WarehouseRepository warehouseRepository,
        BayRepository bayRepository,
        TripRepository tripRepository,
        WorkRepository workRepository,
        LoadRepository loadRepository)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _truckRepository = truckRepository;
        _hubRepository = hubRepository;
        _warehouseRepository = warehouseRepository;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _loadRepository = loadRepository;
        _modelState = modelState;
    }
    
    public async Task<List<Pellet>> GetUnclaimedAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var allPellets = await (_pelletRepository
                .GetUnclaimed(truckCompany))
            .ToListAsync(cancellationToken);

        return allPellets;
    }

    public async Task<List<Pellet>> GetUnclaimedAsync(Hub hub, CancellationToken cancellationToken)
    {
        List<Pellet> allPellets = [];
        
        var bays = _bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var bay in bays)
        {
            allPellets.AddRange(
                await (_pelletRepository
                    .GetUnclaimed(bay)
                    .ToListAsync(cancellationToken))
            );
        }

        var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
        if (warehouse != null)
        {
            allPellets.AddRange(
                await (_pelletRepository
                    .GetUnclaimed(warehouse)
                    .ToListAsync(cancellationToken))
            );
        }
        else
        {
            _logger.LogError("Hub \n({@Hub})\n did not have a Warehouse assigned.",
                hub);
        }
        
        return allPellets;
    }

    private async Task SetPelletsAsync(Load load, long count, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var allPellets = await GetUnclaimedAsync(truckCompany, cancellationToken);
            
        if (allPellets.Count <= 0)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) did not have any unclaimed pellets assigned.",
                truckCompany);

            return;
        }
            
        if (allPellets.Count <= count)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) to fetch Pellets from had less than or an equal number ({@Count}) of unclaimed pellets assigned as the given count ({@Count}).",
                truckCompany,
                allPellets.Count,
                count);

            count = allPellets.Count;
        }
            
        var pellets = allPellets
            .OrderBy(x => _modelState
                .Random())
            .Take((int) count)
            .ToList();

        foreach (var pellet in pellets)
        {
            await _pelletRepository.SetAsync(pellet, load, cancellationToken);
            await _pelletRepository.UnsetAsync(pellet, truckCompany, cancellationToken);
            // Removing pellets from truckCompany since they will leave for the hub now
        }
    }
    
    private async Task SetPelletsAsync(Load load, long count, Hub hub, CancellationToken cancellationToken)
    {
        var allPellets = await GetUnclaimedAsync(hub, cancellationToken);
            
        if (allPellets.Count <= 0)
        {
            _logger.LogInformation("Hub ({@Hub}) did not have any unclaimed pellets assigned.",
                hub);

            return;
        }
            
        if (allPellets.Count <= count)
        {
            _logger.LogInformation("Hub ({@Hub}) to fetch Pellets from had less than or an equal number ({@Count}) of unclaimed pellets assigned as the given count ({@Count}).",
                hub,
                allPellets.Count,
                count);

            count = allPellets.Count;
        }
            
        var pellets = allPellets
            .OrderBy(x => _modelState
                .Random())
            .Take((int) count)
            .ToList();

        foreach (var pellet in pellets)
        {
            await _pelletRepository.SetAsync(pellet, load, cancellationToken);
        }
    }
    
    public async Task SetPelletsAsync(Load load, long count, CancellationToken cancellationToken)
    {
        if (load.LoadType == LoadType.DropOff)
        {
            var truckCompany = await _truckCompanyRepository.GetAsync(load, cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("Load ({@Load}) did not have a Truck Company assigned.",
                    load);

                return;
            }
            
            await SetPelletsAsync(load, count, truckCompany, cancellationToken);
        }
        else
        {
            var hub = await _hubRepository.GetAsync(load, cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Load ({@Load}) did not have a Hub assigned.",
                    load);

                return;
            }
            
            await SetPelletsAsync(load, count, hub, cancellationToken);
        }
    }

    private async Task<bool> HasPelletAsync(Bay bay, Pellet pellet, CancellationToken cancellationToken)
    {
        return (await _pelletRepository
                   .Get(bay)
                   .FirstOrDefaultAsync(p => p.Id == pellet.Id,
                       cancellationToken))
               != null;
    }
    
    private async Task<bool> HasPelletAsync(Truck truck, Pellet pellet, CancellationToken cancellationToken)
    {
        return (await _pelletRepository
                   .Get(truck)
                   .FirstOrDefaultAsync(p => p.Id == pellet.Id,
                       cancellationToken))
               != null;
    }
    
    private async Task<bool> HasWorkAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        return (await _workRepository
            .GetAsync(pellet, cancellationToken))
               != null;
    }

    private async Task DropOff(Pellet pellet, Truck truck, Warehouse warehouse, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, truck, cancellationToken);
        await _pelletRepository.SetAsync(pellet, warehouse, cancellationToken);
        
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }
    
    private async Task Fetch(Pellet pellet, Warehouse warehouse, Bay bay, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, warehouse, cancellationToken);
        await _pelletRepository.SetAsync(pellet, bay, cancellationToken);
        
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }
    
    private async Task PickUp(Pellet pellet, Bay bay, Truck truck, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, bay, cancellationToken);
        await _pelletRepository.SetAsync(pellet, truck, cancellationToken);
        
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }

    public async Task AlertDroppedOffAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Trip assigned.",
                bay);
            
            return;
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return;
        }

        if (_pelletRepository.Get(truck).FirstOrDefault(p => p.Id == pellet.Id) == null)
        {
            _logger.LogError("Cannot unload Pellet \n({@Pellet})\n for this Trip \n({@Trip})\n from this Truck " +
                             "\n({@Truck})\n at this Bay \n({@Bay})\n since its Inventory does not have the Pellet assigned.",
                pellet,
                trip,
                truck,
                bay);

            return;
        }

        var hub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Bay \n({@Bay})\n did not have a Hub assigned.",
                bay);
            
            return;
        }
        
        var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
        if (warehouse == null)
        {
            _logger.LogError("Hub \n({@Hub})\n did not have a Warehouse assigned.",
                warehouse);
            
            return;
        }

        await DropOff(pellet, truck, warehouse, cancellationToken);
    }

    public async Task AlertFetchedAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetAsync(pellet, cancellationToken);
        if (warehouse == null)
        {
            _logger.LogError("Pellet ({@Pellet}) did not have a Warehouse assigned to Fetch from.",
                warehouse);
            
            return;
        }

        var warehouseHub = await _hubRepository.GetAsync(warehouse, cancellationToken);
        if (warehouseHub == null)
        {
            _logger.LogError("Warehouse ({@Warehouse}) did not have a Hub assigned.",
                warehouseHub);
            
            return;
        }
        
        var bayHub = await _hubRepository.GetAsync(bay, cancellationToken);
        if (bayHub == null)
        {
            _logger.LogError("Bay ({@Bay}) did not have a Hub assigned.",
                bayHub);
            
            return;
        }

        if (warehouseHub.Id != bayHub.Id)
        {
            _logger.LogError("Pellet ({@Pellet}) at this Warehouse ({@Warehouse}) at this Hub ({@Hub}) could not be fetched for this Bay ({@Bay}) because its Hub ({@Hub}) does not have the Pellet Warehouse assigned.",
                pellet,
                warehouse,
                warehouseHub,
                bay,
                bayHub);
            
            return;
        }

        await Fetch(pellet, warehouse, bay, cancellationToken);
    }

    public async Task AlertPickedUpAsync(Pellet pellet, Trip trip, CancellationToken cancellationToken)
    {
        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return;
        }

        var bayPellets = _pelletRepository.Get(bay);
        if (!bayPellets.Any(p => p.Id == pellet.Id))
        {
            _logger.LogError("Cannot load Pellet ({@Pellet}) for this Trip ({@Trip}) since its Bay ({@Bay}) does not have the pellet assigned.",
                pellet,
                trip,
                bay);
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return;
        }
        
        if (_pelletRepository.Get(truck).FirstOrDefault(p => p.Id == pellet.Id) != null)
        {
            _logger.LogError("Cannot unload Pellet \n({@Pellet})\n for this Trip \n({@Trip})\n onto this Truck " +
                             "\n({@Truck})\n at this Bay \n({@Bay})\n since its Inventory already has the Pellet assigned.",
                pellet,
                trip,
                truck,
                bay);

            return;
        }
        
        await PickUp(pellet, bay, truck, cancellationToken);
    }
    
    public async Task<List<Pellet>> GetDropOffPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        var dropOffLoad = await _loadRepository.GetAsync(trip, LoadType.DropOff, cancellationToken);
        if (dropOffLoad == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Drop-Off.",
                trip);
            
            return [];
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return [];
        }
        
        var dropOffPellets = new List<Pellet>();

        foreach (var dropOffPellet in dropOffLoad.Pellets)
        {
            if (await HasPelletAsync(truck, dropOffPellet, cancellationToken) &&
                !await HasWorkAsync(dropOffPellet, cancellationToken))
            {
                dropOffPellets.Add(dropOffPellet);
            }
        }

        return dropOffPellets;
    }

    public async Task<List<Pellet>> GetFetchPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                trip);
            
            return [];
        }

        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return [];
        }
        
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return [];
        }
        
        var fetchPellets = new List<Pellet>();

        foreach (var pickUpPellet in pickUpLoad.Pellets)
        {
            if (!await HasPelletAsync(truck, pickUpPellet, cancellationToken) &&
                !await HasPelletAsync(bay, pickUpPellet, cancellationToken) &&
                !await HasWorkAsync(pickUpPellet, cancellationToken))   // TODO: Check if Bay Hub matches Warehouse Hub
            {
                fetchPellets.Add(pickUpPellet);
            }
        }

        return fetchPellets;
    }

    public async Task<List<Pellet>> GetPickUpPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        var pickUpLoad = await _loadRepository.GetAsync(trip, LoadType.PickUp, cancellationToken);
        if (pickUpLoad == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                trip);

            return [];
        }

        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return [];
        }
        
        var pickUpPellets = new List<Pellet>();

        foreach (var pickUpPellet in pickUpLoad.Pellets)
        {
            if (!await HasPelletAsync(truck, pickUpPellet, cancellationToken) &&
                !await HasWorkAsync(pickUpPellet, cancellationToken))
            {
                pickUpPellets.Add(pickUpPellet);
            }
        }

        return pickUpPellets;
    }

    public async Task<Pellet?> GetNextDropOffAsync(Trip trip, CancellationToken cancellationToken)
    {
        var pellets = await GetDropOffPelletsAsync(trip, cancellationToken);

        return pellets.Count <= 0 ? null : pellets[0];
    }

    public async Task<Pellet?> GetNextFetchAsync(Trip trip, CancellationToken cancellationToken)
    {
        var pellets = await GetFetchPelletsAsync(trip, cancellationToken);

        return pellets.Count <= 0 ? null : pellets[0];
    }
    
    public async Task<Pellet?> GetNextPickUpAsync(Trip trip, CancellationToken cancellationToken)
    {
        var pellets = await GetPickUpPelletsAsync(trip, cancellationToken);

        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return null;
        }

        pellets = pellets
            .Where(p => p.BayId == bay.Id)
            .ToList();

        return pellets.Count <= 0 ? null : pellets[0];
    }

    public async Task<bool> HasDropOffPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        return (await GetDropOffPelletsAsync(trip, cancellationToken)).Count > 0;
    }

    public async Task<bool> HasFetchPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        return (await GetFetchPelletsAsync(trip, cancellationToken)).Count > 0;
    }

    public async Task<bool> HasPickUpPelletsAsync(Trip trip, CancellationToken cancellationToken)
    {
        return (await GetPickUpPelletsAsync(trip, cancellationToken)).Count > 0;
    }

    public async Task CompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(trip, cancellationToken);
        if (truck == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Truck assigned.",
                trip);
            
            return;
        }
        
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("Truck \n({@Truck})\n for this Trip \n({@Trip})\n did not have a TruckCompany assigned.",
                truck,
                trip);
            
            return;
        }

        var pellets = _pelletRepository.Get(truck)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var pellet in pellets)
        {
            await _pelletRepository.UnsetAsync(pellet, truck, cancellationToken);
            await _pelletRepository.SetAsync(pellet, truckCompany, cancellationToken);
        }
    }
}