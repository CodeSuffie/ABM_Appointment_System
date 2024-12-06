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
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
        _workRepository = workRepository;
        _loadRepository = loadRepository;
        _modelState = modelState;
    }
    
    public async Task SetPelletsAsync(Load load, long count, CancellationToken cancellationToken)
    {
        List<Pellet> allPellets;
        
        var truckCompany = await _truckCompanyRepository.GetAsync(load, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("Load ({@Load}) did not have a Truck Company assigned.",
                load);

            return;
        }
        
        var hub = await _hubRepository.GetAsync(load, cancellationToken);
        if (hub == null)
        {
            _logger.LogError("Load ({@Load}) did not have a Hub assigned.",
                load);

            return;
        }
        
        if (load.LoadType == LoadType.DropOff)
        {
            allPellets = await (_pelletRepository.GetUnclaimed(truckCompany))
                .ToListAsync(cancellationToken);
            
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
                await _pelletRepository.AddAsync(pellet, load, cancellationToken);
                await _pelletRepository.UnsetAsync(pellet, truckCompany, cancellationToken);
                    // Removing pellets from truckCompany since they will leave for the hub now
            }
        }
        else
        {
            var bays = _bayRepository.Get(hub)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            allPellets = [];

            await foreach (var bay in bays)
            {
                allPellets.AddRange(await (_pelletRepository.GetUnclaimed(bay).ToListAsync(cancellationToken)));
            }
            
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
                await _pelletRepository.AddAsync(pellet, load, cancellationToken);
            }
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
    
    private async Task<bool> HasWorkAsync(Pellet pellet, CancellationToken cancellationToken)
    {
        return (await _workRepository
            .GetAsync(pellet, cancellationToken))
               != null;
    }

    private async Task DropOff(Pellet pellet, Load load, Bay bay, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, load, cancellationToken);
        await _pelletRepository.SetAsync(pellet, bay, cancellationToken);
        
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }
    
    private async Task Fetch(Pellet pellet, Bay bayStart, Bay bayEnd, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, bayStart, cancellationToken);
        await _pelletRepository.SetAsync(pellet, bayEnd, cancellationToken);
        
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }
    
    private async Task PickUp(Pellet pellet, Bay bay, Load load, CancellationToken cancellationToken)
    {
        await _pelletRepository.UnsetAsync(pellet, bay, cancellationToken);
        await _pelletRepository.AddAsync(pellet, load, cancellationToken);
        
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Load assigned as Inventory.",
                trip);
            
            return;
        }

        if (inventoryLoad.Pellets.FirstOrDefault(p => p.Id == pellet.Id) == null)
        {
            _logger.LogError("Cannot unload Pellet \n({@Pellet})\n for this Trip \n({@Trip})\n at this Bay \n({@Bay})\n since its Inventory Load \n({@Load})\n does not have the Pellet assigned.",
                pellet,
                trip,
                bay,
                inventoryLoad);

            return;
        }

        await DropOff(pellet, inventoryLoad, bay, cancellationToken);
    }

    public async Task AlertFetchedAsync(Pellet pellet, Bay bayEnd, CancellationToken cancellationToken)
    {
        var bayStart = await _bayRepository.GetAsync(pellet, cancellationToken);
        if (bayStart == null)
        {
            _logger.LogError("Pellet ({@Pellet}) did not have a bay assigned to Fetch from.",
                pellet);
            
            return;
        }

        if (bayStart.Id == bayEnd.Id)
        {
            _logger.LogError("Cannot fetch Pellet ({@Pellet}) for Bay ({@Bay}) since it was already there.",
                pellet,
                bayEnd);
            
            return;
        }

        await Fetch(pellet, bayStart, bayEnd, cancellationToken);
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Load assigned as Inventory.",
                trip);
            
            return;
        }
        
        if (inventoryLoad.Pellets.FirstOrDefault(p => p.Id == pellet.Id) != null)
        {
            _logger.LogError("Cannot load Pellet ({@Pellet}) for this Trip ({@Trip}) at this Bay ({@Bay}) since its Inventory Load ({@Load}) already has the Pellet assigned.",
                pellet,
                trip,
                bay,
                inventoryLoad);

            return;
        }
        
        await PickUp(pellet, bay, inventoryLoad, cancellationToken);
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Load assigned as Inventory.",
                trip);
            
            return [];
        }

        var pellets = new List<Pellet>();

        foreach (var dropOffPellet in dropOffLoad.Pellets)
        {
            if (inventoryLoad.Pellets.FirstOrDefault(p => p.Id == dropOffPellet.Id) != null &&
                !await HasWorkAsync(dropOffPellet, cancellationToken))
            {
                pellets.Add(dropOffPellet);
            }
        }

        return pellets;
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Load assigned as Inventory.",
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
        
        var pellets = new List<Pellet>();

        foreach (var pickUpPellet in pickUpLoad.Pellets)
        {
            if (inventoryLoad.Pellets.FirstOrDefault(p => p.Id == pickUpPellet.Id) == null &&
                !await HasPelletAsync(bay, pickUpPellet, cancellationToken) &&
                !await HasWorkAsync(pickUpPellet, cancellationToken))
            {
                pellets.Add(pickUpPellet);
            }
        }

        return pellets;
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Load assigned as Inventory.",
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
        
        var pellets = new List<Pellet>();

        foreach (var pickUpPellet in pickUpLoad.Pellets)
        {
            if (inventoryLoad.Pellets.FirstOrDefault(p => p.Id == pickUpPellet.Id) == null &&
                !await HasWorkAsync(pickUpPellet, cancellationToken))
            {
                pellets.Add(pickUpPellet);
            }
        }

        return pellets;
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
        
        var inventoryLoad = await _loadRepository.GetAsync(trip, LoadType.Inventory, cancellationToken);
        if (inventoryLoad == null)
        {
            _logger.LogError("Trip \n({@Trip})\n did not have a Load assigned as Inventory.",
                trip);
            
            return;
        }

        foreach (var pellet in inventoryLoad.Pellets)
        {
            await _pelletRepository.SetAsync(pellet, truckCompany, cancellationToken);
        }
    }
}