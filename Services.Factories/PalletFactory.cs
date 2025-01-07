using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class PalletFactory : IFactoryService<Pallet>
{
    private readonly ILogger<PalletFactory> _logger;
    private readonly PalletRepository _palletRepository;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly TruckCompanyFactory _truckCompanyFactory;
    private readonly HubRepository _hubRepository;
    private readonly HubFactory _hubFactory;
    private readonly WarehouseRepository _warehouseRepository;
    private readonly ModelState _modelState;
    
    public PalletFactory(
        ILogger<PalletFactory> logger,
        PalletRepository palletRepository,
        TruckCompanyRepository truckCompanyRepository,
        TruckCompanyFactory truckCompanyFactory,
        HubRepository hubRepository,
        HubFactory hubFactory,
        WarehouseRepository warehouseRepository,
        ModelState modelState)
    {
        _logger = logger;
        _palletRepository = palletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _truckCompanyFactory = truckCompanyFactory;
        _hubRepository = hubRepository;
        _hubFactory = hubFactory;
        _warehouseRepository = warehouseRepository;
        _modelState = modelState;    
    }
    
    private int GetDifficulty()
    {
        var averageDeviation = _modelState.ModelConfig.PalletDifficultyDeviation;
        var deviation = _modelState.Random(averageDeviation * 2) - averageDeviation;
        return _modelState.ModelConfig.PalletAverageDifficulty + deviation;
    }
    
    public async Task<Pallet?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var pallet = new Pallet
        {
            Difficulty = GetDifficulty()
        };
        
        await _palletRepository.AddAsync(pallet, cancellationToken);

        return pallet;
    }
    
    public async Task AddNewTruckCompanyPalletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var truckCompany = await _truckCompanyFactory.SelectTruckCompanyAsync(cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("Could not select a TruckCompany for this new Pallet.");

                continue;
            }
            
            var pallet = await GetNewObjectAsync(cancellationToken);
            if (pallet == null)
            {
                _logger.LogError("Pallet could not be created.");

                continue;
            }
            
            _logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n for this Pallet \n({@Pallet})", truckCompany, pallet);
            await _palletRepository.SetAsync(pallet, truckCompany, cancellationToken);
            
            _logger.LogInformation("New Pallet created: Pallet={@Pallet}", pallet);
        }
    }

    public async Task AddNewWarehousePalletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var pallet = new Pallet
            {
                Difficulty = GetDifficulty()
            };
            await _palletRepository.AddAsync(pallet, cancellationToken);

            var hub = await _hubFactory.SelectHubAsync(cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Could not select a Hub for this new Pallet.");

                continue;
            }

            var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
            if (warehouse == null)
            {
                _logger.LogError("Hub ({@Hub}) did not have a Warehouse assigned for this new Pallet.", hub);

                continue;
            }

            _logger.LogDebug("Setting Warehouse \n({@Warehouse})\n for this Pallet \n({@Pallet})", warehouse, pallet);
            await _palletRepository.SetAsync(pallet, warehouse, cancellationToken);

            _logger.LogInformation("New Pallet created: Pallet={@Pallet}", pallet);
        }
    }
    
    public async Task<List<Pallet>> GetUnclaimedAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var allPallets = await (_palletRepository
                .GetUnclaimed(truckCompany))
            .ToListAsync(cancellationToken);

        return allPallets;
    }

    public async Task<List<Pallet>> GetUnclaimedAsync(Hub hub, CancellationToken cancellationToken)
    {
        List<Pallet> allPallets = [];

        var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
        if (warehouse != null)
        {
            allPallets.AddRange(
                await (_palletRepository
                    .GetUnclaimed(warehouse)
                    .ToListAsync(cancellationToken))
            );
        }
        else
        {
            _logger.LogError("Hub \n({@Hub})\n did not have a Warehouse assigned.", hub);
        }
        
        return allPallets;
    }

    private async Task SetPalletsAsync(Load load, long count, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var allPallets = await GetUnclaimedAsync(truckCompany, cancellationToken);
            
        if (allPallets.Count <= 0)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) did not have any unclaimed pallets assigned.", truckCompany);

            return;
        }
            
        if (allPallets.Count <= count)
        {
            _logger.LogInformation("TruckCompany ({@TruckCompany}) to fetch Pallets from had less than or an equal number ({@Count}) of unclaimed pallets assigned as the given count ({@Count}).", truckCompany, allPallets.Count, count);

            count = allPallets.Count;
        }
            
        var pallets = allPallets
            .OrderBy(_ => _modelState
                .Random())
            .Take((int) count)
            .ToList();

        foreach (var pallet in pallets)
        {
            await _palletRepository.SetAsync(pallet, load, cancellationToken);
            await _palletRepository.UnsetAsync(pallet, truckCompany, cancellationToken);
            // Removing pallets from truckCompany since they will leave for the hub now
        }
    }
    
    private async Task SetPalletsAsync(Load load, long count, Hub hub, CancellationToken cancellationToken)
    {
        var allPallets = await GetUnclaimedAsync(hub, cancellationToken);
            
        if (allPallets.Count <= 0)
        {
            _logger.LogInformation("Hub ({@Hub}) did not have any unclaimed pallets assigned.", hub);

            return;
        }
            
        if (allPallets.Count <= count)
        {
            _logger.LogInformation("Hub ({@Hub}) to fetch Pallets from had less than or an equal number ({@Count}) of unclaimed pallets assigned as the given count ({@Count}).", hub, allPallets.Count, count);

            count = allPallets.Count;
        }
            
        var pallets = allPallets
            .OrderBy(_ => _modelState
                .Random())
            .Take((int) count)
            .ToList();

        foreach (var pallet in pallets)
        {
            await _palletRepository.SetAsync(pallet, load, cancellationToken);
        }
    }
    
    public async Task SetPalletsAsync(Load load, long count, CancellationToken cancellationToken)
    {
        if (load.LoadType == LoadType.DropOff)
        {
            var truckCompany = await _truckCompanyRepository.GetAsync(load, cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("Load ({@Load}) did not have a Truck Company assigned.", load);

                return;
            }
            
            await SetPalletsAsync(load, count, truckCompany, cancellationToken);
        }
        else
        {
            var hub = await _hubRepository.GetAsync(load, cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Load ({@Load}) did not have a Hub assigned.", load);

                return;
            }
            
            await SetPalletsAsync(load, count, hub, cancellationToken);
        }
    }
}