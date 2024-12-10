using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;

namespace Services.StufferServices;

public sealed class StufferService(
    ILogger<StufferService> logger,
    HubService hubService,
    HubShiftService hubShiftService,
    StufferRepository stufferRepository,
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
        //throw new NotImplementedException();
    }

    public async Task AlertFreeAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        //throw new NotImplementedException();
    }
}