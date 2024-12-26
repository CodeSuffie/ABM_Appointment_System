using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class AdminStaffFactory(
    ILogger<AdminStaffFactory> logger,
    HubFactory hubFactory,
    AdminShiftFactory adminShiftFactory,
    AdminStaffRepository adminStaffRepository,
    ModelState modelState) : IFactoryService<AdminStaff>
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.AdminStaffSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.AdminStaffAverageSpeed + deviation;
    }
    
    public async Task<AdminStaff?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubFactory.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new AdminStaff.");

            return null;
        }
        
        logger.LogDebug("Hub \n({@Hub})\n was selected for the new AdminStaff.", hub);
        
        var adminStaff = new AdminStaff
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.AdminStaffAverageWorkDays,
            Speed = GetSpeed(),
            AverageShiftLength = modelState.AgentConfig.AdminHubShiftAverageLength
        };

        await adminStaffRepository.AddAsync(adminStaff, cancellationToken);
        
        logger.LogDebug("Setting HubShifts for this AdminStaff \n({@AdminStaff})", adminStaff);
        await adminShiftFactory.GetNewObjectsAsync(adminStaff, cancellationToken);

        return adminStaff;
    }
}