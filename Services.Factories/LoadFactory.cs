using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class LoadFactory : IFactoryService<Load>
{
    private readonly ILogger<LoadFactory> _logger;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly HubService _hubService;
    private readonly PelletService _pelletService;
    private readonly LoadRepository _loadRepository;
    private readonly ModelState _modelState;
    
    public LoadFactory(
        ILogger<LoadFactory> logger,
        TruckCompanyRepository truckCompanyRepository,
        HubService hubService,
        PelletService pelletService,
        LoadRepository loadRepository,
        ModelState modelState)
    {
        _logger = logger;
        _truckCompanyRepository = truckCompanyRepository;
        _hubService = hubService;
        _pelletService = pelletService;
        _loadRepository = loadRepository;
        _modelState = modelState;
    }

    public async Task<Load?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var load = new Load();

        await _loadRepository.AddAsync(load, cancellationToken);

        return load;
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

        var load = await GetNewObjectAsync(cancellationToken);
        if (load == null)
        {
            _logger.LogError("Load could not be created.");

            return null;
        }
        
        _logger.LogDebug("Setting LoadType ({LoadType}) for this Load \n({@Load}).",
            LoadType.DropOff,
            load);
        await _loadRepository.SetAsync(load, LoadType.DropOff, cancellationToken);
        
        _logger.LogDebug("Setting TruckCompany ({@TruckCompany}) for this Load \n({@Load}).",
            truckCompany,
            load);
        await _loadRepository.SetAsync(load, truckCompany, cancellationToken);
        
        _logger.LogDebug("Setting Hub ({@Hub}) for this Load \n({@Load}).",
            hub,
            load);
        await _loadRepository.SetAsync(load, hub, cancellationToken);
        
        _logger.LogDebug("Setting Pellets for this Load \n({@Load})\n for this Truck \n({@Truck})\n and this Hub \n({@Hub}).",
            load,
            truck,
            hub);
        await _pelletService.SetPelletsAsync(load, truck.Capacity, cancellationToken);

        if (load.Pellets.Count != 0) return load;
        
        _logger.LogError("Load \n({@Load})\n could not be assigned any Pellets.",
            load);
        
        _logger.LogDebug("Removing this Load \n({@Load}).",
            load);
        await _loadRepository.RemoveAsync(load, cancellationToken);
        
        return null;
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
        
        var load = await GetNewObjectAsync(cancellationToken);
        if (load == null)
        {
            _logger.LogError("Load could not be created.");

            return null;
        }
        
        _logger.LogDebug("Setting LoadType ({LoadType}) for this Load \n({@Load}).",
            LoadType.PickUp,
            load);
        await _loadRepository.SetAsync(load, LoadType.PickUp, cancellationToken);
        
        _logger.LogDebug("Setting TruckCompany ({@TruckCompany}) for this Load \n({@Load}).",
            truckCompany,
            load);
        await _loadRepository.SetAsync(load, truckCompany, cancellationToken);
        
        _logger.LogDebug("Setting Hub ({@Hub}) for this Load \n({@Load}).",
            hub,
            load);
        await _loadRepository.SetAsync(load, hub, cancellationToken);
        
        _logger.LogDebug("Setting Pellets for this Load \n({@Load})\n for this Truck \n({@Truck})\n and this Hub \n({@Hub}).",
            load,
            truck,
            hub);
        await _pelletService.SetPelletsAsync(load, truck.Capacity, cancellationToken);

        if (load.Pellets.Count != 0) return load;
        
        _logger.LogError("Load \n({@Load})\n could not be assigned any Pellets.",
            load);
        
        _logger.LogDebug("Removing this Load \n({@Load}).",
            load);
        await _loadRepository.RemoveAsync(load, cancellationToken);
        
        return null;
    }
    
    public async Task<Load?> GetNewPickUpAsync(Truck truck, CancellationToken cancellationToken)
    {
        var hub = await _hubService.SelectHubAsync(cancellationToken);
        
        if (hub != null) return await GetNewPickUpAsync(truck, hub, cancellationToken);
        
        _logger.LogError("No Hub could be selected for the new Load.");

        return null;

    }
}