using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.BayServices;
using Services.HubServices;
using Services.ModelServices;

namespace Services.BayStaffServices;

public sealed class BayStaffService(
    ILogger<BayStaffService> logger,
    HubService hubService,
    HubRepository hubRepository,
    BayRepository bayRepository,
    BayService bayService,
    WorkService workService,
    TripRepository tripRepository,
    LoadRepository loadRepository,
    ModelState modelState) 
{
    public async Task<BayStaff?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        if (hub == null)
        {
            logger.LogError("No Hub could be selected for the new BayStaff");

            return null;
        }
        logger.LogDebug("Hub ({@Hub}) was selected for the new BayStaff.",
            hub);
        
        var bayStaff = new BayStaff
        {
            Hub = hub,
            WorkChance = modelState.AgentConfig.BayStaffAverageWorkDays,
            AverageShiftLength = modelState.AgentConfig.BayShiftAverageLength
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
                
                logger.LogInformation("Bay ({@Bay}) just completed Work of type {WorkType} in this Step ({Step}).",
                    bay,
                    WorkType.DropOff,
                    modelState.ModelTime);
                
                logger.LogDebug("Alerting Drop-Off Completed to this Bay ({@Bay})...",
                    bay);
                await bayService.AlertDroppedOffAsync(bay, cancellationToken);
                
                break;
            
            case WorkType.Fetch:
                
                logger.LogInformation("Bay ({@Bay}) just completed Work of type {WorkType} in this Step ({Step}).",
                    bay,
                    WorkType.Fetch,
                    modelState.ModelTime);
                
                logger.LogDebug("Alerting Fetch Completed to this Bay ({@Bay})...",
                    bay);
                await bayService.AlertFetchedAsync(bay, cancellationToken);
                
                break;
            
            case WorkType.PickUp when
                bay.BayStatus is
                    not BayStatus.Free and          // In hopes of not spamming a Bay with messages from all the separate Staff members
                    not BayStatus.Claimed and
                    not BayStatus.DroppingOffStarted:
                
                logger.LogInformation("Bay ({@Bay}) just completed Work of type {WorkType} in this Step ({Step}).",
                    bay,
                    WorkType.PickUp,
                    modelState.ModelTime);
                
                logger.LogDebug("Alerting Pick-Up Completed to this Bay ({@Bay})...",
                    bay);
                await bayService.AlertPickedUpAsync(bay, cancellationToken);
                
                break;
        }
    }
    
    public async Task AlertFreeAsync(BayStaff bayStaff, Bay bay, CancellationToken cancellationToken)
    {
        if (bay.BayStatus == BayStatus.Closed)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore be opened in this Step ({Step}).",
                bayStaff,
                bay,
                BayStatus.Closed,
                modelState.ModelTime);
            
            logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay ({@Bay})...",
                BayStatus.Free,
                bay);
            await bayRepository.SetAsync(bay, BayStatus.Free, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.Free)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore alert the Bay is free this Step ({Step}).",
                bayStaff,
                bay,
                BayStatus.Free,
                modelState.ModelTime);
            
            logger.LogDebug("Alerting Free to this Bay ({@Bay})...",
                bay);
            await bayService.AlertFreeAsync(bay, cancellationToken);
        }
        
        if (bay.BayStatus == BayStatus.Claimed)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Drop-Off Work this Step ({Step}).",
                bayStaff,
                bay,
                BayStatus.Claimed,
                modelState.ModelTime);
            
            logger.LogDebug("Setting BayStatus {@BayStatus} for this Bay ({@Bay})...",
                BayStatus.DroppingOffStarted,
                bay);
            await bayRepository.SetAsync(bay, BayStatus.DroppingOffStarted, cancellationToken);
            
            logger.LogDebug("Starting Drop-Off Work for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                bayStaff,
                bay);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
            
            return;
        }

        if (bay.BayStatus is
            BayStatus.DroppingOffStarted or
            BayStatus.WaitingFetchStart)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Fetch Work in this Step ({Step}).",
                bayStaff,
                bay,
                bay.BayStatus,
                modelState.ModelTime);
            
            logger.LogDebug("Starting Fetch Work for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                bayStaff,
                bay);
            await StartFetchAsync(bay, bayStaff, cancellationToken);
            
            return;
        }

        if (bay.BayStatus is
            BayStatus.FetchStarted or
            BayStatus.FetchFinished)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Drop-Off Work in this Step ({Step}).",
                bayStaff,
                bay,
                bay.BayStatus,
                modelState.ModelTime);
            
            logger.LogDebug("Starting Drop-Off Work for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                bayStaff,
                bay);
            await StartDropOffAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.PickUpStarted)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and can therefore start Pick-Up Work in this Step ({Step}).",
                bayStaff,
                bay,
                BayStatus.PickUpStarted,
                modelState.ModelTime);
            
            logger.LogDebug("Starting Pick-Up Work for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                bayStaff,
                bay);
            await StartPickUpAsync(bay, bayStaff, cancellationToken);
        }

        if (bay.BayStatus == BayStatus.WaitingFetch)
        {
            logger.LogInformation("BayStaff ({@BayStaff}) is working at Bay ({@Bay}) with assigned BayStatus {@BayStatus}" +
                                  "and therefore has no work to be assigned in this Stef ({Step})",
                bayStaff,
                bay,
                BayStatus.WaitingFetch,
                modelState.ModelTime);
            
            logger.LogDebug("BayStaff ({@BayStaff}) will remain idle...",
                bay);
        }
    }
    
    public async Task StartDropOffAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding Work of type {WorkType} for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
            WorkType.DropOff,
            bayStaff,
            bay);
        await workService.AddAsync(bay, bayStaff, WorkType.DropOff, cancellationToken);
        
        logger.LogDebug("Adapting the Workload of other active Drop-Off Work for this Bay ({@Bay})...",
            bay);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
    
    public async Task StartFetchAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(bay, cancellationToken);
        if (trip == null)
        {
            logger.LogError("Bay ({@Bay}) did not have a Trip assigned to start Fetch Work for.",
                bay);
            
            return;
        }
        
        var pickUpLoad = await loadRepository.GetPickUpAsync(trip, cancellationToken);
        if (pickUpLoad == null)
        {
            logger.LogInformation("Trip ({@Trip}) did not have a Load assigned to Pick-Up.",
                trip);

            return;
        }
        
        var pickUpLoadBay = await bayRepository.GetAsync(pickUpLoad, cancellationToken);
        if (pickUpLoadBay == null)
        {
            logger.LogInformation("Load ({@Load}) to Pick-Up for this Trip ({@Trip}) did not have a bay assigned to Fetch it from.",
                pickUpLoad,
                trip);

            if (bay.BayStatus == BayStatus.WaitingFetchStart)
            {
                logger.LogInformation("Bay ({@Bay}) has assigned BayStatus {@BayStatus}" +
                                      "and can therefore not wait longer to Fetch the Load ({@Load})" +
                                      "for this Trip ({@Trip}).",
                    bay,
                    BayStatus.WaitingFetchStart,
                    pickUpLoad,
                    trip);
                
                logger.LogDebug("Unsetting Pick-Up Load ({@Load}) for this Trip ({@Trip})...",
                    pickUpLoad,
                    trip);
                await loadRepository.UnsetPickUpAsync(pickUpLoad, trip, cancellationToken);
            }
            else
            {
                logger.LogInformation("Bay ({@Bay}) has assigned BayStatus {@BayStatus}" +
                                      "and can therefore wait longer to Fetch the Load ({@Load})" +
                                      "for this Trip ({@Trip}).",
                    bay,
                    bay.BayStatus,
                    pickUpLoad,
                    trip);
                
                logger.LogDebug("Starting Drop-Off Work for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                    bayStaff,
                    bay);
                await StartDropOffAsync(bay, bayStaff, cancellationToken);

                return;
            }
        }
        else if (pickUpLoadBay.Id != bay.Id)
        {
            logger.LogInformation("Load ({@Load}) to Pick-Up for this Trip ({@Trip}) is not assigned to the same Bay ({@Bay}) as this Bay ({@Bay}).",
                pickUpLoad,
                trip,
                pickUpLoadBay,
                bay);
            
            logger.LogDebug("Adding Work of type {WorkType} for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
                WorkType.Fetch,
                bayStaff,
                bay);
            await workService.AddAsync(bay, bayStaff, WorkType.Fetch, cancellationToken);
            
            return;
        }
        else
        {
            logger.LogInformation("Load ({@Load}) to Pick-Up for this Trip ({@Trip}) is assigned to the same Bay ({@Bay}) as this Bay ({@Bay}).",
                pickUpLoad,
                trip,
                pickUpLoadBay,
                bay);
        }
            
        logger.LogInformation("Fetch Work could not be started for this Trip ({@Trip}) and is therefore completed.",
            trip);
        
        logger.LogDebug("Alerting Fetch Work has completed for this Bay ({@Bay})...",
            bay);
        await bayService.AlertFetchedAsync(bay, cancellationToken);
            
        logger.LogDebug("Alerting Free for this BayStaff ({@BayStaff}) to this Bay ({@Bay})...",
            bayStaff,
            bay);
        await AlertFreeAsync(bayStaff, bay, cancellationToken);
    }

    public async Task StartPickUpAsync(Bay bay, BayStaff bayStaff, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding Work of type {WorkType} for this BayStaff ({@BayStaff}) at this Bay ({@Bay})...",
            WorkType.PickUp,
            bayStaff,
            bay);
        await workService.AddAsync(bay, bayStaff, WorkType.PickUp, cancellationToken);
        
        logger.LogDebug("Adapting the Workload of other active Pick-Up Work for this Bay ({@Bay})...",
            bay);
        await workService.AdaptWorkLoadAsync(bay, cancellationToken);
    }
}
