using System.Diagnostics.Metrics;
using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;
using Services.TruckCompanyServices;

namespace Services.PelletServices;

public sealed class PelletCreation
{
    private readonly ILogger<PelletCreation> _logger;
    private readonly PelletRepository _pelletRepository;
    private readonly TruckCompanyService _truckCompanyService;
    private readonly HubService _hubService;
    private readonly BayService _bayService;
    private readonly ModelState _modelState;
    private readonly UpDownCounter<int> _unclaimedPellets;
    
    public PelletCreation(
        ILogger<PelletCreation> logger,
        PelletRepository pelletRepository,
        TruckCompanyService truckCompanyService,
        HubService hubService,
        BayService bayService,
        ModelState modelState,
        Meter meter)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyService = truckCompanyService;
        _hubService = hubService;
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
}