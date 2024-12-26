using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class StufferFactory(
    ILogger<StufferFactory> logger,
    HubFactory hubFactory,
    StufferShiftFactory stufferShiftFactory,
    StufferRepository stufferRepository,
    ModelState modelState) : IFactoryService<Stuffer>
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.StufferSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.StufferAverageSpeed + deviation;
    }
    
    public async Task<Stuffer?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubFactory.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new Picker.");

            return null;
        }
        
        logger.LogDebug("Hub \n({@Hub})\n was selected for the new Stuffer.", hub);
        
        var stuffer = new Stuffer
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.StufferAverageWorkDays,
            Speed = GetSpeed(),
            Experience = modelState.RandomDouble() + 0.5,
            AverageShiftLength = modelState.AgentConfig.StufferHubShiftAverageLength
        };
        
        await stufferRepository.AddAsync(stuffer, cancellationToken);
        
        logger.LogDebug("Setting HubShifts for this Stuffer \n({@Stuffer})", stuffer);
        await stufferShiftFactory.GetNewObjectsAsync(stuffer, cancellationToken);

        return stuffer;
    }
}