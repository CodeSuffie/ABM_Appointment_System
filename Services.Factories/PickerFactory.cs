using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class PickerFactory(
    ILogger<PickerFactory> logger,
    PickerRepository pickerRepository,
    HubService hubService,
    PickerShiftFactory pickerShiftFactory,
    ModelState modelState) : IFactoryService<Picker>
{
    private int GetSpeed()
    {
        var averageDeviation = modelState.AgentConfig.PickerSpeedDeviation;
        var deviation = modelState.Random(averageDeviation * 2) - averageDeviation;
        return modelState.AgentConfig.PickerAverageSpeed + deviation;
    }

    public async Task<Picker?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new Picker.");

            return null;
        }
        
        logger.LogDebug("Hub \n({@Hub})\n was selected for the new Picker.",
            hub);
        
        var picker = new Picker
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.PickerAverageWorkDays,
            Speed = GetSpeed(),
            Experience = modelState.RandomDouble() + 0.5,
            AverageShiftLength = modelState.AgentConfig.PickerHubShiftAverageLength
        };
        
        await pickerRepository.AddAsync(picker, cancellationToken);
        
        logger.LogDebug("Setting HubShifts for this Picker \n({@Picker})",
            picker);
        await pickerShiftFactory.GetNewObjectsAsync(picker, cancellationToken);

        return picker;
    }
}