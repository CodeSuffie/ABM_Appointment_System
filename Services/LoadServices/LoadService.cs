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
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedLoads;

    public LoadService(ILogger<LoadService> logger,
        TruckCompanyRepository truckCompanyRepository,
        HubService hubService,
        PelletService pelletService,
        LoadRepository loadRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _truckCompanyRepository = truckCompanyRepository;
        _hubService = hubService;
        _pelletService = pelletService;
        _loadRepository = loadRepository;
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
}