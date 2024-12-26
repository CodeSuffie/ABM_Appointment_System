using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class BayShiftFactory(
    ILogger<BayShiftFactory> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    BayStaffRepository bayStaffRepository,
    BayShiftRepository bayShiftRepository,
    BayRepository bayRepository,
    BayFactory bayFactory,
    ModelState modelState) : IShiftFactoryService<BayStaff, BayShift, OperatingHour>
{
    public TimeSpan? GetStartTime(BayStaff bayStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - bayStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("BayStaff \n({@BayStaff})\n its ShiftLength \n({TimeSpan})\n is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan}).", bayStaff, bayStaff.AverageShiftLength, operatingHour, operatingHour.Duration);

            return null;
        }

        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }

    public async Task<double?> GetWorkChanceAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        
        if (hub != null) return bayStaff.WorkChance / hub.WorkChance;
        
        logger.LogError("BayStaff \n({@BayStaff})\n did not have a Hub assigned to get the OperatingHourChance for.", bayStaff);

        return null;
    }
    
    public async Task<BayShift?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var bayShift = new BayShift();
        
        await bayShiftRepository.AddAsync(bayShift, cancellationToken);

        return bayShift;
    }
    
    public async Task<BayShift?> GetNewObjectAsync(BayStaff bayStaff, Bay bay, TimeSpan startTime, TimeSpan duration, CancellationToken cancellationToken)
    {
        var bayShift = await GetNewObjectAsync(cancellationToken);
        if (bayShift == null)
        {
            logger.LogError("BayShift could not be created.");

            return null;
        }
        
        logger.LogDebug("Setting this StartTime ({Step}) for this BayShift \n({@BayShift}).", startTime, bayShift);
        await bayShiftRepository.SetStartAsync(bayShift, startTime, cancellationToken);
        
        logger.LogDebug("Setting this Duration ({Step}) for this BayShift \n({@BayShift}).", duration, bayShift);
        await bayShiftRepository.SetDurationAsync(bayShift, duration, cancellationToken);
        
        logger.LogDebug("Setting this BayStaff \n({@BayStaff})\n for this BayShift \n({@BayShift}).", bayStaff, bayShift);
        await bayShiftRepository.SetAsync(bayShift, bayStaff, cancellationToken);
        
        logger.LogDebug("Setting this Bay \n({@Bay})\n for this BayShift \n({@BayShift}).", bay, bayShift);
        await bayShiftRepository.SetAsync(bayShift, bay, cancellationToken);

        return bayShift;
    }

    public async Task<BayShift?> GetNewObjectAsync(BayStaff bayStaff, OperatingHour operatingHour, CancellationToken cancellationToken)
    {
        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            logger.LogError("This function cannot be called with Appointment System Mode");

            return null;
        }
        
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("BayStaff \n({@BayStaff})\n did not have a Hub assigned to create BayShifts for.", bayStaff);

            return null;
        }
        
        var bay = await bayFactory.SelectBayAsync(hub, cancellationToken);
        if (bay == null)
        {
            logger.LogError("The Hub \n({@Hub})\n did not have a Bay to assign to the new BayShift for this BayStaff \n({@BayStaff}).", hub, bayStaff);

            return null;
        }
        
        var startTime = GetStartTime(bayStaff, operatingHour);
        if (startTime != null)
            return await GetNewObjectAsync(bayStaff, bay, (TimeSpan)startTime, bayStaff.AverageShiftLength, cancellationToken);
        
        logger.LogError("No start time could be assigned to the new BayShift for this BayStaff \n({@BayStaff})\n during this OperatingHour \n({@OperatingHour}).", bayStaff, operatingHour);

        return null;
    }
    
    public async Task GetNewObjectsAsync(BayStaff bayStaff, CancellationToken cancellationToken)
    {
        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            logger.LogError("This function cannot be called with Appointment System Mode.");

            return;
        }
        
        var hub = await hubRepository.GetAsync(bayStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("BayStaff \n({@BayStaff})\n did not have a Hub assigned to create BayShifts for.", bayStaff);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(bayStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this BayStaff \n({@BayStaff})\n during this OperatingHour \n({@OperatingHour}).", bayStaff, operatingHour);

                continue;
            }

            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("BayStaff \n({@BayStaff})\n will not have a BayShift during this OperatingHour \n({@OperatingHour}).", bayStaff, operatingHour);
                
                continue;
            }

            var bayShift = await GetNewObjectAsync(bayStaff, operatingHour, cancellationToken);
            if (bayShift == null)
            {
                logger.LogError("No new BayShift could be created for this BayStaff \n({@BayStaff})\n during this OperatingHour \n({@OperatingHour}).", bayStaff, operatingHour);

                continue;
            }

            await bayStaffRepository.AddAsync(bayStaff, bayShift, cancellationToken);
            logger.LogInformation("New BayShift created for this BayStaff \n({@BayStaff})\n during this OperatingHour \n({@OperatingHour})\n: BayShift={@BayShift}", bayStaff, operatingHour, bayShift);
        }
    }
    
    public async Task GetNewObjectsAsync(CancellationToken cancellationToken)
    {
        if (!modelState.ModelConfig.AppointmentSystemMode)
        {
            logger.LogError("This function cannot be called without Appointment System Mode.");

            return;
        }
        
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var hub in hubs)
        {
            var bayStaffs = await bayStaffRepository.Get(hub).ToListAsync(cancellationToken);
            
            if (bayStaffs.Count == 0) continue;
            
            var operatingHours = operatingHourRepository.Get(hub)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            await foreach (var operatingHour in operatingHours)
            {
                var bays = await bayRepository.Get(hub).ToListAsync(cancellationToken);

                
                for (var i = 0; i < bays.Count; i++)
                {
                    var bayStaff = bayStaffs[i % bayStaffs.Count];
                    
                    logger.LogDebug("Setting BayShift for this BayStaff \n({@BayStaff}).", bayStaff);
                    await GetNewObjectAsync(
                        bayStaff, 
                        bays[i], 
                        operatingHour.StartTime, 
                        operatingHour.Duration, 
                        cancellationToken);
                }
            }
        }
    }
}