using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.HubServices;
using Services.ModelServices;

namespace Services.PickerServices;

public sealed class PickerService(
    ILogger<PickerService> logger,
    HubService hubService,
    HubShiftService hubShiftService,
    PickerRepository pickerRepository,
    ModelState modelState)
{
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
            AverageShiftLength = modelState.AgentConfig.PickerHubShiftAverageLength
        };
        
        await pickerRepository.AddAsync(picker, cancellationToken);
        
        logger.LogDebug("Setting HubShifts for this Picker \n({@Picker})",
            picker);
        await hubShiftService.GetNewObjectsAsync(picker, cancellationToken);

        return picker;
    }
}