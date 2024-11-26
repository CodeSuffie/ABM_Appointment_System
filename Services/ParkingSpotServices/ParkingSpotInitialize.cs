using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.ModelServices;

namespace Services.ParkingSpotServices;

public sealed class ParkingSpotInitialize(
    ILogger<ParkingSpotInitialize> logger,
    ParkingSpotService parkingSpotService,
    HubRepository hubRepository,
    ModelState modelState) : IInitializationService
{
    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = await parkingSpotService.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New ParkingSpot created: ParkingSpot={@ParkingSpot}", parkingSpot);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.ParkingSpotLocations.GetLength(0); i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}