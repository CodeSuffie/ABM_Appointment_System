using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.ModelServices;
using Settings;

namespace Services.BayServices;

public sealed class BayService(
    ModelDbContext context,
    BayShiftService bayShiftService)
{
    public async Task<Bay> GetNewObjectAsync(Hub hub, CancellationToken cancellationToken)
    {
        var bay = new Bay
        {
            XSize = 1,
            YSize = 1,
            Opened = false,
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
    
    public async Task<IQueryable<BayShift>> GetShiftsForBayAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = context.BayShifts
            .Where(x => x.BayId == bay.Id);

        return shifts;
    }

    public async Task AlertFreeAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: If Truck at my Bay, If PickUp Load is not available at this Bay, and no one else is fetching it, fetch the Load
        // TODO: If Truck at my Bay, continue handling their Trip
    }

    public async Task AlertShiftEndAsync(Bay bay, CancellationToken cancellationToken)
    {
        var shifts = await bayShiftService.GetCurrentShiftsAsync(bay, cancellationToken);
        if (shifts.Count > 1) return;
        
        bay.Opened = false;
        
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AlertShiftStartAsync(Bay bay, CancellationToken cancellationToken)
    {
        bay.Opened = true;
        
        await context.SaveChangesAsync(cancellationToken);
    }
}
