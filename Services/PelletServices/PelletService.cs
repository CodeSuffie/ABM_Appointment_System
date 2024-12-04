using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services.PelletServices;

public sealed class PelletService
{
    private readonly ILogger<PelletService> _logger;
    private readonly ModelState _modelState;
    private readonly PelletRepository _pelletRepository;
    private readonly TruckCompanyService _truckCompanyService;
    private readonly TruckCompanyRepository _truckCompanyRepository;
    private readonly HubService _hubService;
    private readonly HubRepository _hubRepository;
    private readonly BayService _bayService;
    private readonly UpDownCounter<int> _unclaimedPellets;

    public PelletService(
        ILogger<PelletService> logger,
        ModelState modelState,
        PelletRepository pelletRepository,
        TruckCompanyService truckCompanyService,
        TruckCompanyRepository truckCompanyRepository,
        HubService hubService,
        HubRepository hubRepository,
        BayService bayService,
        Meter meter)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _truckCompanyService = truckCompanyService;
        _hubService = hubService;
        _hubRepository = hubRepository;
        _bayService = bayService;
        _modelState = modelState;
        
        _unclaimedPellets =
            meter.CreateUpDownCounter<int>("pellet-unclaimed", "Pellet", "#Pellets unclaimed (excl. completed).");
    }
    
    public async Task AddNewTruckCompanyPelletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var pellet = new Pellet();
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
    
    public async Task AddNewBayPelletsAsync(int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            var pellet = new Pellet();
            await _pelletRepository.AddAsync(pellet, cancellationToken);

            var hub = await _hubService.SelectHubAsync(cancellationToken);
            if (hub == null)
            {
                _logger.LogError("Could not select a Hub for this new Pellet.");

                continue;
            }
            
            var bay = await _bayService.SelectBayAsync(hub, cancellationToken);
            if (bay == null)
            {
                _logger.LogError("Could not select a Bay for this Hub ({@Hub}) for this new Pellet.",
                    hub);

                continue;
            }
            
            _logger.LogDebug("Setting Bay \n({@Bay})\n for this Pellet \n({@Pellet})",
                bay,
                pellet);
            await _pelletRepository.SetAsync(pellet, bay, cancellationToken);
            
            _logger.LogInformation("New Pellet created: Pellet={@Pellet}", pellet);
    
            _unclaimedPellets.Add(1);
        }
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
                _logger.LogError("TruckCompany ({@TruckCompany}) did not have any unclaimed pellets assigned.",
                    truckCompany);

                return;
            }
        }
        else
        {
            allPellets = await (_pelletRepository.GetUnclaimed(hub))
                .ToListAsync(cancellationToken);
            
            if (allPellets.Count <= 0)
            {
                _logger.LogError("Hub ({@Hub}) did not have any unclaimed pellets assigned.",
                    hub);

                return;
            }
        }
        
        if (allPellets.Count <= count)
        {
            _logger.LogError("Location to fetch Pellets from had less than or an equal number of unclaimed pellets assigned as the given count ({@Count}).",
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

    public async Task AlertDroppedOffAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AlertFetchedAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AlertPickedUpAsync(Pellet pellet, Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Pellet?> GetNextDropOffAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Pellet?> GetNextFetchAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Pellet?> GetNextPickUpAsync(Trip trip, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}