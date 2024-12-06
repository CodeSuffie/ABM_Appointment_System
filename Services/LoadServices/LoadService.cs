using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.PelletServices;
using Services.TruckCompanyServices;

namespace Services.LoadServices;

public sealed class LoadService
{
    private readonly ILogger<LoadService> _logger;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly HubService _hubService;
    private readonly PelletService _pelletService;
    private readonly LoadRepository _loadRepository;
    private readonly PelletRepository _pelletRepository;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedLoads;

    public LoadService(ILogger<LoadService> logger,
        TruckCompanyRepository truckCompanyRepository,
        HubService hubService,
        PelletService pelletService,
        LoadRepository loadRepository,
        PelletRepository pelletRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _truckCompanyRepository = truckCompanyRepository;
        _hubService = hubService;
        _pelletService = pelletService;
        _loadRepository = loadRepository;
        _pelletRepository = pelletRepository;
        _modelState = modelState;

        _unclaimedLoads =
            meter.CreateUpDownCounter<int>("load-unclaimed", "Load", "#Loads unclaimed (excl. completed).");
    }

    public async Task<Load?> GetNewDropOffAsync(Truck truck, CancellationToken cancellationToken)
    {
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) to create the new Load for.",
                truck);

            return null;
        }
        
        var hub = await _hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            _logger.LogError("No Hub could be selected for the new Load.");

            return null;
        }
        
        var load = new Load
        {
            LoadType = LoadType.DropOff,
            TruckCompany = truckCompany,
            Hub = hub
        };

        await _loadRepository.AddAsync(load, cancellationToken);
        
        _logger.LogDebug("Setting Pallets for this Load \n({@Load})\n for this Truck \n({@Truck})",
            load,
            truck);
        await _pelletService.SetPelletsAsync(load, truck.Capacity, cancellationToken);

        return load;
    }
    
    public async Task<Load?> GetNewPickUpAsync(Truck truck, Hub hub, CancellationToken cancellationToken)
    {
        var truckCompany = await _truckCompanyRepository.GetAsync(truck, cancellationToken);
        if (truckCompany == null)
        {
            _logger.LogError("No TruckCompany was assigned to the Truck ({@Truck}) to create the new Load for.",
                truck);

            return null;
        }
        
        var load = new Load
        {
            LoadType = LoadType.PickUp,
            TruckCompany = truckCompany,
            Hub = hub
        };
        
        await _loadRepository.AddAsync(load, cancellationToken);
        
        _logger.LogDebug("Setting Pallets for this Load \n({@Load})\n for this Truck \n({@Truck})\n and this Hub \n({@Hub}).",
            load,
            truck,
            hub);
        await _pelletService.SetPelletsAsync(load, truck.Capacity, cancellationToken);

        return load;
    }
    
    public async Task<Load?> GetNewPickUpAsync(Truck truck, CancellationToken cancellationToken)
    {
        var hub = await _hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            _logger.LogError("No Hub could be selected for the new Load.");

            return null;
        }

        return await GetNewPickUpAsync(truck, hub, cancellationToken);
    }
    
    public async Task<Load> GetNewInventoryAsync(Trip trip, CancellationToken cancellationToken)
    {
        var inventory = new Load
        {
            LoadType = LoadType.Inventory,
            Trip = trip,
        };

        await _loadRepository.AddAsync(inventory, cancellationToken);

        return inventory;
    }
    
    public async Task<Load> GetNewInventoryAsync(Trip trip, Load load, CancellationToken cancellationToken)
    {
        var inventory = await GetNewInventoryAsync(trip, cancellationToken);

        foreach (var pellet in load.Pellets)
        {
            await _pelletRepository.AddAsync(pellet, inventory, cancellationToken);
        }

        return inventory;
    }
    
    // public async Task AddNewLoadsAsync(int count, CancellationToken cancellationToken)
    // {
    //     for (var i = 0; i < count; i++)
    //     {
    //         var load = await GetNewObjectAsync(cancellationToken);
    //         if (load == null)
    //         {
    //             _logger.LogError("Could not construct a new Load...");
    //         
    //             return;
    //         }
    //         
    //         await _loadRepository.AddAsync(load, cancellationToken);
    //         _logger.LogInformation("New Load created: Load={@Load}", load);
    //
    //         _unclaimedLoads.Add(1);
    //     }
    // }
    
    // public async Task<Load?> SelectUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    // {
    //     var dropOffs = await (_loadRepository.GetUnclaimedDropOff(truckCompany))
    //         .ToListAsync(cancellationToken);
    //
    //     if (dropOffs.Count <= 0)
    //     {
    //         _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Drop-Off Load assigned.",
    //             truckCompany);
    //
    //         return null;
    //     }
    //     
    //     var dropOff = dropOffs[_modelState.Random(dropOffs.Count)];
    //     _unclaimedLoads.Add(-1);
    //     return dropOff;
    // }
    //
    // public async Task<Load?> SelectUnclaimedPickUpAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    // {
    //     var pickUps = await (_loadRepository.GetUnclaimedPickUp(truckCompany))
    //         .ToListAsync(cancellationToken);
    //
    //     if (pickUps.Count <= 0)
    //     {
    //         _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned.",
    //             truckCompany);
    //
    //         return null;
    //     }
    //     
    //     var pickUp = pickUps[_modelState.Random(pickUps.Count)];
    //     _unclaimedLoads.Add(-1);
    //     return pickUp;
    // }
    //
    // public async Task<Load?> SelectUnclaimedPickUpAsync(Hub hub, TruckCompany truckCompany, CancellationToken cancellationToken)
    // {
    //     var pickUps = await (_loadRepository.GetUnclaimedPickUp(hub, truckCompany))
    //         .ToListAsync(cancellationToken);
    //
    //     if (pickUps.Count <= 0)
    //     {
    //         _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned for Hub \n({@Hub})",
    //             truckCompany,
    //             hub);
    //
    //         return null;
    //     }
    //     
    //     var pickUp = pickUps[_modelState.Random(pickUps.Count)];
    //     _unclaimedLoads.Add(-1);
    //     return pickUp;
    // }
}