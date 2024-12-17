using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services.PelletServices;

public sealed class PelletFactory
{
    private readonly ILogger<PelletFactory> _logger;
    private readonly PelletRepository _pelletRepository;
    private readonly TruckCompanyService _truckCompanyService;
    private readonly HubService _hubService;
    private readonly WarehouseRepository _warehouseRepository;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedPellets;
    
    public PelletFactory(
        ILogger<PelletFactory> logger,
        PelletRepository pelletRepository,
        TruckCompanyService truckCompanyService,
        HubService hubService,
        WarehouseRepository warehouseRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyService = truckCompanyService;
        _hubService = hubService;
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
    
    public async Task AddNewTruckCompanyPelletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var pellet = new Pellet
            {
                Difficulty = GetDifficulty()
            };
            await _pelletRepository.AddAsync(pellet, cancellationToken);

            var truckCompany = await _truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
            if (truckCompany == null)
            {
                _logger.LogError("Could not select a TruckCompany for this new Pellet.");

                continue;
            }
            
            _logger.LogDebug("Setting TruckCompany \n({@TruckCompany})\n for this Pellet \n({@Pellet})",
                truckCompany,
                pellet);
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

            var hub = await _hubService.SelectHubAsync(cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Could not select a Hub for this new Pellet.");

                continue;
            }

            var warehouse = await _warehouseRepository.GetAsync(hub, cancellationToken);
            if (warehouse == null)
            {
                _logger.LogError("Hub ({@Hub}) did not have a Warehouse assigned for this new Pellet.",
                    hub);

                continue;
            }

            _logger.LogDebug("Setting Warehouse \n({@Warehouse})\n for this Pellet \n({@Pellet})",
                warehouse,
                pellet);
            await _pelletRepository.SetAsync(pellet, warehouse, cancellationToken);

            _logger.LogInformation("New Pellet created: Pellet={@Pellet}", pellet);

            _unclaimedPellets.Add(1);
        }
    }
}