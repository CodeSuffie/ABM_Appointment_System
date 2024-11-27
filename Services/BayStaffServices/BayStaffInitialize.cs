using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.BayStaffServices;

public sealed class BayStaffInitialize(
    ILogger<BayStaffInitialize> logger,
    BayStaffService bayStaffService,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var bayStaff = await bayStaffService.GetNewObjectAsync(cancellationToken);
        if (bayStaff == null)
        {
            logger.LogError("Could not construct a new BayStaff...");
            
            return;
        }
        
        logger.LogInformation("New BayStaff created: BayStaff={@BayStaff}", bayStaff);
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.BayStaffCount; i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}