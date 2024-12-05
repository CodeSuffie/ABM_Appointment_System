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
    private readonly HubRepository _hubRepository;
    private readonly BayRepository _bayRepository;
    private readonly TripRepository _tripRepository;
    private readonly LoadRepository _loadRepository;

    public PelletService(
        ILogger<PelletService> logger,
        ModelState modelState,
        PelletRepository pelletRepository,
        TruckCompanyRepository truckCompanyRepository,
        HubRepository hubRepository,
        BayRepository bayRepository,
        TripRepository tripRepository,
        LoadRepository loadRepository)
    {
        _logger = logger;
        _pelletRepository = pelletRepository;
        _truckCompanyRepository = truckCompanyRepository;
        _hubRepository = hubRepository;
        _bayRepository = bayRepository;
        _tripRepository = tripRepository;
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
        }
        
        if (allPellets.Count <= count)
        {
            _logger.LogInformation("Location to fetch Pellets from had less than or an equal number ({@Count}) of unclaimed pellets assigned as the given count ({@Count}).",
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
            await _pelletRepository.SetAsync(pellet, load, cancellationToken);
        }
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
        await _pelletRepository.SetAsync(pellet, load, cancellationToken);
        await _pelletRepository.UnsetWorkAsync(pellet, cancellationToken);
    }

    public async Task AlertDroppedOffAsync(Pellet pellet, Bay bay, CancellationToken cancellationToken)
    {
        var pelletLoad = await _loadRepository.GetAsync(pellet, cancellationToken);
        if (pelletLoad == null)
        {
            _logger.LogError("Pellet ({@Pellet}) did not have a Load assigned to Drop-Off for.",
                pellet);
            
            return;
        }

        if (pelletLoad.LoadType != LoadType.DropOff)
        {
            _logger.LogError("Load ({@Load}) is not a Drop-Off Load.",
                pelletLoad);
            
            return;
        }
        
        var trip = await _tripRepository.GetAsync(pelletLoad, cancellationToken);
        if (trip == null)
        {
            _logger.LogError("Load ({@Load}) did not have a Trip assigned.",
                pelletLoad);
            
            return;
        }
        
        var tripAtBay = await _tripRepository.GetAsync(bay, cancellationToken);
        if (tripAtBay == null)
        {
            _logger.LogError("Bay ({@Bay}) did not have a Trip assigned.",
                bay);
            
            return;
        }

        if (trip.Id != tripAtBay.Id)
        {
            _logger.LogError("Cannot unload Pellet ({@Pellet}) for this Trip ({@Trip}) at this Bay ({@Bay}) since it has a different Trip ({@Trip}) assigned.",
                pellet,
                trip,
                bay,
                tripAtBay);
            
            return;
        }

        await DropOff(pellet, pelletLoad, bay, cancellationToken);
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

        // if (load.LoadType != LoadType.DropOff)
        // {
        //     _logger.LogError("Load ({@Load}) is not a Drop-Off Load.",
        //         load);
        //     
        //     return;
        // }
        // TODO: Maybe validate if the load actually needed to be fetched?

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
        var pelletLoad = await _loadRepository.GetAsync(pellet, cancellationToken);
        if (pelletLoad == null)
        {
            _logger.LogError("Pellet ({@Pellet}) did not have a Load assigned to Pick-Up for.",
                pellet);
            
            return;
        }

        if (pelletLoad.LoadType != LoadType.PickUp)
        {
            _logger.LogError("Load ({@Load}) is not a Pick-Up Load.",
                pelletLoad);
            
            return;
        }
        
        var tripLoad = await _loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (tripLoad == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Pick-Up load assigned.",
                tripLoad);
            
            return;
        }
        
        if (pelletLoad.Id != tripLoad.Id)
        {
            _logger.LogError("Cannot unload Pellet ({@Pellet}) with Load ({@Load}) for this Trip ({@Trip}) since it has a different Load ({@Load}) assigned.",
                pellet,
                pelletLoad,
                trip,
                tripLoad);
            
            return;
        }
        
        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogError("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return;
        }
        
        await PickUp(pellet, bay, pelletLoad, cancellationToken);
    }

    public async Task<Pellet?> GetNextDropOffAsync(Trip trip, CancellationToken cancellationToken)
    {
        var load = await _loadRepository.GetDropOffAsync(trip, cancellationToken);
        if (load == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Drop-Off.",
                trip);
            
            return null;
        }

        var pellets = await _pelletRepository.GetUnclaimed(load)
            .ToListAsync(cancellationToken);

        return pellets.Count <= 0 ? null : pellets[0];
    }

    public async Task<Pellet?> GetNextFetchAsync(Trip trip, CancellationToken cancellationToken)
    {
        var load = await _loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (load == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                trip);
            
            return null;
        }

        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return null;
        }

        var pellets = await _pelletRepository.GetUnclaimed(load)
            .Where(p => p.BayId != bay.Id)
            .ToListAsync(cancellationToken);

        return pellets.Count <= 0 ? null : pellets[0];
    }

    public async Task<Pellet?> GetNextPickUpAsync(Trip trip, CancellationToken cancellationToken)
    {
        var load = await _loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (load == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                trip);
            
            return null;
        }

        var bay = await _bayRepository.GetAsync(trip, cancellationToken);
        if (bay == null)
        {
            _logger.LogInformation("Trip ({@Trip}) did not have a Bay assigned.",
                trip);
            
            return null;
        }

        var pellets = await _pelletRepository.GetUnclaimed(load)
            .Where(p => p.BayId == bay.Id)
            .ToListAsync(cancellationToken);

        return pellets.Count <= 0 ? null : pellets[0];
    }
}