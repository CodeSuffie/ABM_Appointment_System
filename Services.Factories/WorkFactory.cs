using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;

namespace Services.Factories;

public sealed class WorkFactory(
    ILogger<WorkFactory> logger,
    AppointmentRepository appointmentRepository,
    AppointmentSlotRepository appointmentSlotRepository,
    WorkRepository workRepository,
    ModelState modelState) : IFactoryService<Work>
{
    private TimeSpan GetTime(AdminStaff adminStaff)
    {
        return adminStaff.Speed * modelState.ModelConfig.ModelStep;
    }
    
    private TimeSpan GetTime(BayStaff bayStaff, Pellet pellet)
    {
        return (bayStaff.Speed + pellet.Difficulty) * modelState.ModelConfig.ModelStep;
    }
    
    private TimeSpan GetTime(Picker picker, Pellet pellet)
    {
        return (picker.Speed + pellet.Difficulty) * picker.Experience * modelState.ModelConfig.ModelStep;
    }
    
    private TimeSpan GetTime(Stuffer stuffer)
    {
        return stuffer.Speed * stuffer.Experience * modelState.ModelConfig.ModelStep;
    }
    
    public async Task<Work?> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var work = new Work
        {
            StartTime = modelState.ModelTime,
        };

        await workRepository.AddAsync(work, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(TimeSpan? duration, WorkType workType, CancellationToken cancellationToken)
    {
        var minDuration = new TimeSpan(0, 0, 0);
        duration = duration != null && duration < minDuration ? minDuration : duration;

        var work = await GetNewObjectAsync(cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created.");

            return null;
        }

        logger.LogDebug("Setting this Duration ({Step}) for this Work \n({@Work}).", duration, work);
        await workRepository.SetDurationAsync(work, duration, cancellationToken);
        
        logger.LogDebug("Setting this WorkType ({WorkType}) for this Work \n({@Work}).", workType, work);
        await workRepository.SetAsync(work, workType, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Trip trip, TimeSpan? duration, WorkType workType, CancellationToken cancellationToken)
    {
        var work = await GetNewObjectAsync(duration, workType, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip}).", trip);

            return null;
        }

        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        return work;
    }

    public async Task<Work?> GetNewObjectAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        TimeSpan? duration = null;
        var work = await GetNewObjectAsync(duration, workType, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip}).", trip);

            return null;
        }

        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        return work;
    }

    public async Task<Work?> GetNewAppointmentObjectAsync(Trip trip, CancellationToken cancellationToken)
    {
        if (!modelState.ModelConfig.AppointmentSystemMode)
        {
            logger.LogError("This function cannot be called without Appointment System Mode.");

            return null;
        }
        
        var appointment = await appointmentRepository.GetAsync(trip, cancellationToken);
        if (appointment == null)
        {
            logger.LogError("Trip \n({@Trip})\n did not have an Appointment assigned.", trip);

            return null;
        }

        var appointmentSlot = await appointmentSlotRepository.GetAsync(appointment, cancellationToken);
        if (appointmentSlot == null)
        {
            logger.LogError("Appointment \n({@Appointment})\n for this Trip \n({@Trip})\n did not have an AppointmentSlot assigned.", appointment, trip);

            return null;
        }

        var duration = appointmentSlot.StartTime - (modelState.ModelTime + trip.TravelTime);
        var work = await GetNewObjectAsync(duration, WorkType.WaitTravelHub, cancellationToken);

        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip}) within Appointment System Mode.", trip);
            
            return null;
        }
        
        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Trip trip, CancellationToken cancellationToken)
    {
        if (modelState.ModelConfig.AppointmentSystemMode) return await GetNewAppointmentObjectAsync(trip, cancellationToken);

        TimeSpan? duration = null;
        var work = await GetNewObjectAsync(duration, WorkType.TravelHub, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip}).", trip);
            
            return null;
        }
        
        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        TimeSpan? duration = GetTime(adminStaff);
        var work = await GetNewObjectAsync(duration, WorkType.CheckIn, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip})\n and this AdminStaff \n({@AdminStaff}).", trip, adminStaff);

            return null;
        }

        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        logger.LogDebug("Setting this AdminStaff \n({@AdminStaff})\n for this Work \n({@Work}).", adminStaff, work);
        await workRepository.SetAsync(work, adminStaff, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        TimeSpan? duration = null;
        var work = await GetNewObjectAsync(duration, WorkType.Bay, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Trip \n({@Trip})\n and this Bay \n({@Bay}).", trip, bay);

            return null;
        }

        logger.LogDebug("Setting this Trip \n({@Trip})\n for this Work \n({@Work}).", trip, work);
        await workRepository.SetAsync(work, trip, cancellationToken);

        logger.LogDebug("Setting this Bay \n({@Bay})\n for this Work \n({@Work}).", bay, work);
        await workRepository.SetAsync(work, bay, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Bay bay, BayStaff bayStaff, Pellet pellet, WorkType workType, CancellationToken cancellationToken)
    {
        var duration = GetTime(bayStaff, pellet);
        var work = await GetNewObjectAsync(duration, workType, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this BayStaff \n({@BayStaff})\n at this Bay \n({@Bay})\n for this Pellet \n({@Pellet}).", bayStaff, bay, pellet);

            return null;
        }

        logger.LogDebug("Setting this Bay \n({@Bay})\n for this Work \n({@Work}).", bay, work);
        await workRepository.SetAsync(work, bay, cancellationToken);
        
        logger.LogDebug("Setting this BayStaff \n({@BayStaff})\n for this Work \n({@Work}).", bayStaff, work);
        await workRepository.SetAsync(work, bayStaff, cancellationToken);
        
        logger.LogDebug("Setting this Pellet \n({@Pellet})\n for this Work \n({@Work}).", pellet, work);
        await workRepository.SetAsync(work, pellet, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Bay bay, Picker picker, Pellet pellet, CancellationToken cancellationToken)
    {
        var duration = GetTime(picker, pellet);
        var work = await GetNewObjectAsync(duration, WorkType.Fetch, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Picker \n({@Picker})\n at this Bay \n({@Bay})\n for this Pellet \n({@Pellet}).", picker, bay, pellet);

            return null;
        }

        logger.LogDebug("Setting this Bay \n({@Bay})\n for this Work \n({@Work}).", bay, work);
        await workRepository.SetAsync(work, bay, cancellationToken);
        
        logger.LogDebug("Setting this Picker \n({@Picker})\n for this Work \n({@Work}).", picker, work);
        await workRepository.SetAsync(work, picker, cancellationToken);
        
        logger.LogDebug("Setting this Pellet \n({@Pellet})\n for this Work \n({@Work}).", pellet, work);
        await workRepository.SetAsync(work, pellet, cancellationToken);

        return work;
    }
    
    public async Task<Work?> GetNewObjectAsync(Bay bay, Stuffer stuffer, Pellet pellet, CancellationToken cancellationToken)
    {
        var duration = GetTime(stuffer);
        var work = await GetNewObjectAsync(duration, WorkType.Stuff, cancellationToken);
        if (work == null)
        {
            logger.LogError("Work could not be created for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay})\n for this Pellet \n({@Pellet}).", stuffer, bay, pellet);

            return null;
        }
        
        logger.LogDebug("Setting this Bay \n({@Bay})\n for this Work \n({@Work}).", bay, work);
        await workRepository.SetAsync(work, bay, cancellationToken);
        
        logger.LogDebug("Setting this Stuffer \n({@Stuffer})\n for this Work \n({@Work}).", stuffer, work);
        await workRepository.SetAsync(work, stuffer, cancellationToken);
        
        logger.LogDebug("Setting this Pellet \n({@Pellet})\n for this Work \n({@Work}).", pellet, work);
        await workRepository.SetAsync(work, pellet, cancellationToken);

        return work;
    }
}