using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Settings;

namespace Services.BayServices;

public sealed class BayService(ModelDbContext context)
{
    public async Task<Bay> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            XSize = 1,
            YSize = 1,
            Hub = hub,
        };

        return bay;
    }

    public async Task<Bay> SelectBayByStaff(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var bays = await context.Bays
            .Where(x => x.HubId == bayStaff.Hub.Id)
            .ToListAsync(cancellationToken);

        if (bays.Count <= 0) 
            throw new Exception("There was no Bay assigned to the Hub of this BayStaff.");

        var bay = bays[ModelConfig.Random.Next(bays.Count)];
        return bay;
    }
}
