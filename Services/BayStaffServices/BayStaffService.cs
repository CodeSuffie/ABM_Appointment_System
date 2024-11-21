using Database.Models;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Settings;

namespace Services.BayStaffServices;

public sealed class BayStaffService(
    HubService hubService,
    HubRepository hubRepository,
    BayRepository bayRepository,
    BayService bayService,
    WorkRepository workRepository,
    WorkService workService,
    TripRepository tripRepository,
    LoadRepository loadRepository) 
{
    public async Task<BayStaff> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        
        var bayStaff = new BayStaff
        {
            Hub = hub,
            WorkChance = AgentConfig.BayStaffAverageWorkDays,
            AverageShiftLength = AgentConfig.BayShiftAverageLength
        };

        return bayStaff;
    }
    
    public async Task<double> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        
        return bayStaff.WorkChance / hub.OperatingChance;
    }
    
    public async Task AlertWorkCompleteAsync(WorkType workType, Bay bay, CancellationToken cancellationToken)
    {
        switch (workType)
        {
            case WorkType.DropOff when
                bay.BayStatus is 
                    not BayStatus.WaitingFetchStart and
                    not BayStatus.WaitingFetch and  // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.PickUpStarted:
                await bayService.AlertDroppedOffAsync(bay, cancellationToken);
                break;
            
            case WorkType.Fetch:
                await bayService.AlertFetchedAsync(bay, cancellationToken);
                break;
            
            case WorkType.PickUp when
                bay.BayStatus is
                    not BayStatus.Free and          // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.Claimed and
                    not BayStatus.DroppingOffStarted:
                await bayService.AlertPickedUpAsync(bay, cancellationToken);
                break;
        }
    }
    
    public async Task AlertFreeAsync(BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.Closed)
        {
            await bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.Free)
        {
            await bayService.AlertFreeAsync(bay, cancellationToken);
        }
        
        if (bay.BayStatus == BayStatus.Claimed)
        {
            await bayRepository.SetAsync(bay, BayStatus.DroppingOffStarted, cancellationToken);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
            return;
        }

        if (bay.BayStatus is
            BayStatus.DroppingOffStarted or
            BayStatus.WaitingFetchStart)
        {
            await StartFetchAsync(bay, bayStaff, cancellationToken);
            return;
        }

        if (bay.BayStatus is
            BayStatus.FetchStarted or
            BayStatus.FetchFinished)
        {
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.PickUpStarted)
        {
            await StartPickUpAsync(bay, bayStaff, cancellationToken);
        }
    }
    
    public async Task StartDropOffAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workRepository.AddAsync(bay, bayStaff, WorkType.DropOff, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
            throw new Exception("Cannot start a Fetch Work Job for a BayStaff at a Bay that has no Trip assigned.");
        
        var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
        
        if (pickUpLoad != null)
        {
            var pickUpLoadBay = await bayRepository.GetAsync(pickUpLoad, cancellationToken);
            
            if (pickUpLoadBay == null)
            {
                if (bay.BayStatus == BayStatus.WaitingFetchStart)
                {
                    await bayService.AlertPickedUpAsync(bay, cancellationToken);
                }
                else
                {
                    // TODO: Log that Load has not arrived yet
                }
                
            }
            
            else if (pickUpLoadBay.Id != bay.Id)
            {
                await workRepository.AddAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
                return;
            }
        }
        
        await bayService.AlertFetchedAsync(bay, cancellationToken);            // There is no PickUp Load assigned to this Trip, or it is already at the Bay 
        await AlertFreeAsync(bayStaff, bay, cancellationToken);     // Tell me what to do next
    }

    public async Task StartPickUpAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        await workRepository.AddAsync(bay, bayStaff, WorkType.PickUp, cancellationToken);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
}
