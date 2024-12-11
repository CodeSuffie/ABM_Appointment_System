using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;
using Services.PelletServices;

namespace Services.StufferServices;

public sealed class StufferService(
    ILogger<StufferService> logger,
    HubService hubService,
    HubShiftService hubShiftService,
    StufferRepository stufferRepository,
    WorkRepository workRepository,
    WorkService workService,
    PelletRepository pelletRepository,
    BayRepository bayRepository,
    HubRepository hubRepository,
    PelletService pelletService,
    ModelState modelState)
{
    public async Task<Stuffer?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new Picker.");

            return null;
        }
        
        logger.LogDebug("Hub \n({@Hub})\n was selected for the new Stuffer.",
            hub);
        
        var stuffer = new Stuffer
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.StufferAverageWorkDays,
            AverageShiftLength = modelState.AgentConfig.StufferHubShiftAverageLength
        };
        
        await stufferRepository.AddAsync(stuffer, cancellationToken);
        
        logger.LogDebug("Setting HubShifts for this Stuffer \n({@Stuffer})",
            stuffer);
        await hubShiftService.GetNewObjectsAsync(stuffer, cancellationToken);

        return stuffer;
    }

    public async Task AlertWorkCompleteAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(stuffer, cancellationToken);
        if (work == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have Work assigned to alert completed for.",
                stuffer);

            return;
        }

        var pellet = await pelletRepository.GetAsync(work, cancellationToken);
        if (pellet == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a Pellet assigned to Stuff.",
                stuffer,
                work);

            return;
        }
        
        var bay = await bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a bay assigned to Stuff the Pellet \n({@Pellet})\n for.",
                stuffer,
                work,
                pellet);

            return;
        }
        
        await pelletService.AlertStuffedAsync(pellet, bay, cancellationToken);
    }

    public async Task AlertFreeAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.",
                stuffer);

            return;
        }

        var bays = bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var stuffPelletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPelletCount = (await pelletService
                    .GetAvailableStuffPelletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayStuffPelletCount <= stuffPelletCount)
            {
                continue;
            }
            
            stuffPelletCount = bayStuffPelletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            logger.LogInformation("Stuffer \n({@Stuffer})\n its assigned Hub \n({@Hub})\n did not have a " +
                                   "Bay with more Pellets assigned to Stuff.",
                stuffer,
                hub);
            
            logger.LogDebug("Stuffer \n({@Stuffer})\n will remain idle...",
                stuffer);

            return;
        }

        await StartStuffAsync(stuffer, bestBay, cancellationToken);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await pelletService.GetNextStuffAsync(bay, cancellationToken);
        if (pellet == null)
        {
            logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Stuff.",
                bay);
            
            logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pellet \n({@Pellet})",
            stuffer,
            bay,
            pellet);
        await workService.AddAsync(bay, stuffer, pellet, cancellationToken);
    }
}