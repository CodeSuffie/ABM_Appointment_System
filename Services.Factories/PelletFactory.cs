using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class PelletFactory : IFactoryService<Pellet>
{
    private readonly ILogger<PelletFactory> _logger;
    private readonly PelletRepository _pelletRepository;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly TruckCompanyFactory _truckCompanyFactory;
    private readonly HubRepository _hubRepository;
    private readonly HubFactory _hubFactory;
    private readonly WarehouseRepository _warehouseRepository;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedPellets;
    
    public PelletFactory(
        ILogger<PelletFactory> logger,
        PelletRepository pelletRepository,
        TruckCompanyRepository truckCompanyRepository,
        TruckCompanyFactory truckCompanyFactory,
        HubRepository hubRepository,
        HubFactory hubFactory,
        WarehouseRepository warehouseRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _truckCompanyFactory = truckCompanyFactory;
        _hubRepository = hubRepository;
        _hubFactory = hubFactory;
        _warehouseRepository = warehouseRepository;
        _modelState = modelState;    
        
        _unclaimedPellets =
            meter.CreateUpDownCounter<int>("pellet-unclaimed", "Pellet", "#Pellets unclaimed (excl. completed).");
    }
    
    private int GetDifficulty()
    {
        var averageDeviation = _modelState.ModelConfig.PelletDifficultyDeviation;
        var deviation = _modelState.Random(averageDeviation * 2) - averageDeviation;
        return _modelState.ModelConfig.PelletAverageDifficulty + deviation;
    }
    
    public async Task<Pellet?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var pellet = new Pellet
        {
            Difficulty = GetDifficulty()
        };
        
        await _pelletRepository.AddAsync(pellet, cancellationToken);

        return pellet;
    }
    
    public async Task AddNewTruckCompanyPelletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var truckCompany = await _truckCompanyFactory.SelectTruckCompanyAsync(cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("Could not select a TruckCompany for this new Pellet.");

                continue;
            }
            
            var pellet = await GetNewObjectAsync(cancellationToken);
            if (pellet == null)
            {
                _logger.LogError("Pellet could not be created.");

                continue;
            }
            
            _logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n for this Pellet \n({@Pellet})", truckCompany, pellet);
            await _pelletRepository.SetAsync(pellet, truckCompany, cancellationToken);
            
            _logger.LogInformation("New Pellet created: Pellet={@Pellet}", pellet);
    
            _unclaimedPellets.Add(1);
        }
    }

    public async Task AddNewWarehousePelletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var pellet = new Pellet
            {
                Difficulty = GetDifficulty()
            };
            await _pelletRepository.AddAsync(pellet, cancellationToken);

            var hub = await _hubFactory.SelectHubAsync(cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Could not select a Hub for this new Pellet.");

                continue;
            }

            var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
            if (warehouse == null)
            {
                _logger.LogError("Hub ({@Hub}) did not have a Warehouse assigned for this new Pellet.", hub);

                continue;
            }

            _logger.LogDebug("Setting Warehouse \n({@Warehouse})\n for this Pellet \n({@Pellet})", warehouse, pellet);
            await _pelletRepository.SetAsync(pellet, warehouse, cancellationToken);

            _logger.LogInformation("New Pellet created: Pellet={@Pellet}", pellet);

            _unclaimedPellets.Add(1);
        }
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
            _logger.LogError("Hub \n({@Hub})\n did not have a Warehouse assigned.", hub);
        }
        
        return allPellets;
    }

    private async Task SetPelletsAsync(Load load, long count, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var allPellets = await GetUnclaimedAsync(truckCompany, cancellationToken);
            
        if (allPellets.Count <= 0)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) did not have any unclaimed pellets assigned.", truckCompany);

            return;
        }
            
        if (allPellets.Count <= count)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) to fetch Pellets from had less than or an equal number ({@Count}) of unclaimed pellets assigned as the given count ({@Count}).", truckCompany, allPellets.Count, count);

            count = allPellets.Count;
        }
            
        var pellets = allPellets
            .OrderBy(_ => _modelState
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
            _logger.LogInformation("Hub ({@Hub}) did not have any unclaimed pellets assigned.", hub);

            return;
        }
            
        if (allPellets.Count <= count)
        {
            _logger.LogInformation("Hub ({@Hub}) to fetch Pellets from had less than or an equal number ({@Count}) of unclaimed pellets assigned as the given count ({@Count}).", hub, allPellets.Count, count);

            count = allPellets.Count;
        }
            
        var pellets = allPellets
            .OrderBy(_ => _modelState
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
                _logger.LogError("Load ({@Load}) did not have a Truck Company assigned.", load);

                return;
            }
            
            await SetPelletsAsync(load, count, truckCompany, cancellationToken);
        }
        else
        {
            var hub = await _hubRepository.GetAsync(load, cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Load ({@Load}) did not have a Hub assigned.", load);

                return;
            }
            
            await SetPelletsAsync(load, count, hub, cancellationToken);
        }
    }
}