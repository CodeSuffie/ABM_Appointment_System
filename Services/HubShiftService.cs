using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class HubShiftService(
    ILogger<HubShiftService> logger,
    HubRepository hubRepository,
    OperatingHourRepository operatingHourRepository,
    AdminStaffRepository adminStaffRepository,
    PickerRepository pickerRepository,
    StufferRepository stufferRepository,
    HubShiftRepository hubShiftRepository,
    ModelState modelState) 
{
    private TimeSpan? GetStartTime(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - adminStaff.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan})",
                adminStaff,
                adminStaff.AverageShiftLength,
                operatingHour,
                operatingHour.Duration);

            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }
    
    private TimeSpan? GetStartTime(Picker picker, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - picker.AverageShiftLength;

        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Picker \n({@Picker})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan})",
                picker,
                picker.AverageShiftLength,
                operatingHour,
                operatingHour.Duration);

            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);

        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }
    
    private TimeSpan? GetStartTime(Stuffer stuffer, OperatingHour operatingHour)
    {
        var maxShiftStart = operatingHour.Duration - stuffer.AverageShiftLength;
    
        if (maxShiftStart < TimeSpan.Zero)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its ShiftLength \n({TimeSpan})\n " +
                            "is longer than this OperatingHour \n({@OperatingHour})\n its Length \n({TimeSpan})",
                stuffer,
                stuffer.AverageShiftLength,
                operatingHour,
                operatingHour.Duration);
    
            return null;
        }
            
        var shiftBlock = (maxShiftStart / 3).Hours;
            
        var shiftHour = shiftBlock * modelState.Random(3);
    
        return operatingHour.StartTime + new TimeSpan(shiftHour, 0, 0);
    }
    
    public async Task<double?> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        
        if (hub != null) return adminStaff.WorkChance / hub.WorkChance;
        
        logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to get the OperatingHourChance for.",
            adminStaff);

        return null;
    }
    
    public async Task<double?> GetWorkChanceAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(picker, cancellationToken);
        
        if (hub != null) return picker.WorkChance / hub.WorkChance;
        
        logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to get the OperatingHourChance for.",
            picker);

        return null;
    }
    
    public async Task<double?> GetWorkChanceAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        
        if (hub != null) return stuffer.WorkChance / hub.WorkChance;
        
        logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to get the OperatingHourChance for.",
            stuffer);
    
        return null;
    }
    
    public HubShift? GetNewObject(AdminStaff adminStaff, OperatingHour operatingHour)
    {
        var startTime = GetStartTime(adminStaff, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this " +
                            "AdminStaff \n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
                adminStaff,
                operatingHour);

            return null;
        }
        
        var hubShift = new HubShift {
            AdminStaff = adminStaff,
            StartTime = (TimeSpan) startTime,
            Duration = adminStaff.AverageShiftLength
        };

        return hubShift;
    }
    
    public HubShift? GetNewObject(Picker picker, OperatingHour operatingHour)
    {
        var startTime = GetStartTime(picker, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this " +
                            "Picker \n({@Picker})\n during this OperatingHour \n({@OperatingHour})",
                picker,
                operatingHour);

            return null;
        }
        
        var hubShift = new HubShift {
            Picker = picker,
            StartTime = (TimeSpan) startTime,
            Duration = picker.AverageShiftLength
        };

        return hubShift;
    }
    
    public HubShift? GetNewObject(Stuffer stuffer, OperatingHour operatingHour)
    {
        var startTime = GetStartTime(stuffer, operatingHour);
        if (startTime == null)
        {
            logger.LogError("No start time could be assigned to the new HubShift for this " +
                            "Stuffer \n({@Stuffer})\n during this OperatingHour \n({@OperatingHour})",
                stuffer,
                operatingHour);
    
            return null;
        }
        
        var hubShift = new HubShift {
            Stuffer = stuffer,
            StartTime = (TimeSpan) startTime,
            Duration = stuffer.AverageShiftLength
        };
    
        return hubShift;
    }

    public async Task GetNewObjectsAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        if (hub == null)
        {
            logger.LogError("AdminStaff \n({@AdminStaff})\n did not have a Hub assigned to create HubShifts for.",
                adminStaff);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(adminStaff, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this AdminStaff " +
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);

                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("AdminStaff \n({@AdminStaff})\n will not have an HubShift during " +
                                      "this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);
                
                continue;
            }
            
            var hubShift = GetNewObject(adminStaff, operatingHour);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this AdminStaff " +
                                "\n({@AdminStaff})\n during this OperatingHour \n({@OperatingHour})",
                    adminStaff,
                    operatingHour);

                continue;
            }

            await adminStaffRepository.AddAsync(adminStaff, hubShift, cancellationToken);
            logger.LogInformation("New HubShift created for this AdminStaff \n({@AdminStaff})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}",
                adminStaff,
                operatingHour,
                hubShift);
        }
    }
    
    public async Task GetNewObjectsAsync(Picker picker, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(picker, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Picker \n({@Picker})\n did not have a Hub assigned to create HubShifts for.",
                picker);

            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(picker, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this Picker " +
                                "\n({@Picker})\n during this OperatingHour \n({@OperatingHour})",
                    picker,
                    operatingHour);

                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("Picker \n({@Picker})\n will not have an HubShift during " +
                                      "this OperatingHour \n({@OperatingHour})",
                    picker,
                    operatingHour);
                
                continue;
            }
            
            var hubShift = GetNewObject(picker, operatingHour);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this Picker " +
                                "\n({@Picker})\n during this OperatingHour \n({@OperatingHour})",
                    picker,
                    operatingHour);

                continue;
            }

            await pickerRepository.AddAsync(picker, hubShift, cancellationToken);
            logger.LogInformation("New HubShift created for this Picker \n({@Picker})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}",
                picker,
                operatingHour,
                hubShift);
        }
    }
    
    public async Task GetNewObjectsAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to create HubShifts for.",
                stuffer);
    
            return;
        }
        
        var operatingHours = operatingHourRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var operatingHour in operatingHours)
        {
            var workChance = await GetWorkChanceAsync(stuffer, cancellationToken);
            if (workChance == null)
            {
                logger.LogError("WorkChance could not be calculated for this Stuffer " +
                                "\n({@Stuffer})\n during this OperatingHour \n({@OperatingHour})",
                    stuffer,
                    operatingHour);
    
                continue;
            }
            
            if (modelState.RandomDouble() > workChance)
            {
                logger.LogInformation("Stuffer \n({@Stuffer})\n will not have an HubShift during " +
                                      "this OperatingHour \n({@OperatingHour})",
                    stuffer,
                    operatingHour);
                
                continue;
            }
            
            var hubShift = GetNewObject(stuffer, operatingHour);
            if (hubShift == null)
            {
                logger.LogError("No new HubShift could be created for this Stuffer " +
                                "\n({@Stuffer})\n during this OperatingHour \n({@OperatingHour})",
                    stuffer,
                    operatingHour);
    
                continue;
            }
    
            await stufferRepository.AddAsync(stuffer, hubShift, cancellationToken);
            logger.LogInformation("New HubShift created for this Stuffer \n({@Stuffer})\n during this " +
                                  "OperatingHour \n({@OperatingHour})\n: HubShift={@HubShift}",
                stuffer,
                operatingHour,
                hubShift);
        }
    }
    
    private bool IsCurrent(HubShift hubShift)
    {
        var endTime = hubShift.StartTime + hubShift.Duration;
        
        return modelState.ModelTime >= hubShift.StartTime && modelState.ModelTime <= endTime;
    }
    
    public async Task<HubShift?> GetCurrentAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(adminStaff)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this AdminStaff \n({@AdminStaff})\n.",
                shift,
                adminStaff);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this AdminStaff \n({@AdminStaff})\n.",
            adminStaff);
        return null;
    }

    public async Task<HubShift?> GetCurrentAsync(Picker picker, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(picker)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this Picker \n({@Picker})\n.",
                shift,
                picker);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this Picker \n({@Picker})\n.",
            picker);
        return null;
    }
    
    public async Task<HubShift?> GetCurrentAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var shifts = hubShiftRepository.Get(stuffer)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        await foreach (var shift in shifts)
        {
            if (!IsCurrent(shift)) continue;
            
            logger.LogInformation("HubShift \n({@HubShift})\n is currently active for this Stuffer \n({@Stuffer})\n.",
                shift,
                stuffer);
                
            return shift;
        }

        logger.LogInformation("No HubShift is currently active for this Stuffer \n({@Stuffer})\n.",
            stuffer);
        return null;
    }
}