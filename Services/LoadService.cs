using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services;

public class LoadService
{
    private readonly ILogger<LoadService> _logger;
    private readonly TruckCompanyService _truckCompanyService;
    private readonly HubService _hubService;
    private readonly LoadRepository _loadRepository;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedLoads;

    public LoadService(ILogger<LoadService> logger,
        TruckCompanyService truckCompanyService,
        HubService hubService,
        LoadRepository loadRepository,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _truckCompanyService = truckCompanyService;
        _hubService = hubService;
        _loadRepository = loadRepository;
        _modelState = modelState;

        _unclaimedLoads =
            meter.CreateUpDownCounter<int>("load-unclaimed", "Load", "#Loads unclaimed (excl. completed).");
    }

    public async Task<Load?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var truckCompanyStart = await _truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompanyStart == null)
        {
            _logger.LogError("No TruckCompany could be selected for the new Load start location.");

            return null;
        }
        
        var truckCompanyEnd = await _truckCompanyService.SelectTruckCompanyAsync(cancellationToken);
        if (truckCompanyEnd == null)
        {
            _logger.LogError("No TruckCompany could be selected for the new Load end location.");

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
            TruckCompanyStart = truckCompanyStart,
            TruckCompanyEnd = truckCompanyEnd,
            Hub = hub
        };

        return load;
    }
    
    public async Task AddNewLoadsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var load = await GetNewObjectAsync(cancellationToken);
            if (load == null)
            {
                _logger.LogError("Could not construct a new Load...");
            
                return;
            }
            
            await _loadRepository.AddAsync(load, cancellationToken);
            _logger.LogInformation("New Load created: Load={@Load}", load);

            _unclaimedLoads.Add(1);
        }
    }
    
    public async Task<Load?> SelectUnclaimedDropOffAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOffs = await (_loadRepository.GetUnclaimedDropOff(truckCompany))
            .ToListAsync(cancellationToken);

        if (dropOffs.Count <= 0)
        {
            _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Drop-Off Load assigned.",
                truckCompany);

            return null;
        }
        
        var dropOff = dropOffs[_modelState.Random(dropOffs.Count)];
        _unclaimedLoads.Add(-1);
        return dropOff;
    }
    
    public async Task<Load?> SelectUnclaimedPickUpAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (_loadRepository.GetUnclaimedPickUp(truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0)
        {
            _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned.",
                truckCompany);

            return null;
        }
        
        var pickUp = pickUps[_modelState.Random(pickUps.Count)];
        _unclaimedLoads.Add(-1);
        return pickUp;
    }

    public async Task<Load?> SelectUnclaimedPickUpAsync(Hub hub, TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var pickUps = await (_loadRepository.GetUnclaimedPickUp(hub, truckCompany))
            .ToListAsync(cancellationToken);

        if (pickUps.Count <= 0)
        {
            _logger.LogInformation("TruckCompany \n({@TruckCompany})\n did not have an unclaimed Pick-Up Load assigned for Hub \n({@Hub})",
                truckCompany,
                hub);

            return null;
        }
        
        var pickUp = pickUps[_modelState.Random(pickUps.Count)];
        _unclaimedLoads.Add(-1);
        return pickUp;
    }
}