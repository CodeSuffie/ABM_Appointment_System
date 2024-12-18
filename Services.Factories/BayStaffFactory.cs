using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class BayStaffFactory(
    ILogger<BayStaffFactory> logger,
    HubFactory hubFactory,
    BayShiftFactory bayShiftFactory,
    BayStaffRepository bayStaffRepository,
    ModelState modelState) : IFactoryService<BayStaff>
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.BayStaffSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.BayStaffAverageSpeed + deviation;
    }
    
    public async Task<BayStaff?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubFactory.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new BayStaff");

            return null;
        }
        logger.LogDebug("Hub \n({@Hub})\n was selected for the new BayStaff.",
            hub);
        
        var bayStaff = new BayStaff
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.BayStaffAverageWorkDays,
            Speed = GetSpeed(),
            AverageShiftLength = modelState.AgentConfig.BayShiftAverageLength
        };

        await bayStaffRepository.AddAsync(bayStaff, cancellationToken);

        if (modelState.ModelConfig.AppointmentSystemMode) return bayStaff;
        
        logger.LogDebug("Setting BayShifts for this BayStaff \n({@BayStaff})",
            bayStaff);
        await bayShiftFactory.GetNewObjectsAsync(bayStaff, cancellationToken);

        return bayStaff;
    }
}