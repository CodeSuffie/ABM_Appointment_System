using Database.Models;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class WorkService(
    ILogger<WorkService> logger,
    AppointmentRepository appointmentRepository,
    AppointmentSlotRepository appointmentSlotRepository,
    WorkRepository workRepository,
    ModelState modelState)
{
    // TODO: Add timer for Appointment mode for how long BayStaff is working
    
    public bool IsWorkCompleted(Work work)
    {
        if (work.Duration == null)
        {
            logger.LogError("Work \n({@Work})\n does not have a Duration",
                work);

            return false;
        }
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        
        return endTime <= modelState.ModelTime;
    }
    
    private Work GetNew(TimeSpan? duration, WorkType workType)
    {
        var minDuration = new TimeSpan(0, 0, 0);
        duration = duration != null && duration < minDuration ? minDuration : duration;
        
        var work = new Work
        {
            StartTime = modelState.ModelTime,
            Duration = duration,
            WorkType = workType,
        };

        return work;
    }
    
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
    
    public async Task AddAsync(Trip trip, WorkType workType, CancellationToken cancellationToken)
    {
        var work = GetNew(null, workType);

        await workRepository.AddAsync(work, trip, cancellationToken);
    }
    
    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            var appointment = await appointmentRepository.GetAsync(trip, cancellationToken);
            if (appointment == null)
            {
                logger.LogError("Trip \n({@Trip})\n did not have an Appointment assigned.",
                    trip);

                return;
            }

            var appointmentSlot = await appointmentSlotRepository.GetAsync(appointment, cancellationToken);
            if (appointmentSlot == null)
            {
                logger.LogError("Appointment \n({@Appointment})\n for this Trip \n({@Trip})\n did not have an AppointmentSlot assigned.",
                    appointment,
                    trip);

                return;
            }

            var duration = appointmentSlot.StartTime - (modelState.ModelTime + trip.TravelTime);
            var work = GetNew(duration, WorkType.WaitTravelHub);

            await workRepository.AddAsync(work, trip, cancellationToken);
        }
    }
    
    public async Task AddAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var work = GetNew(GetTime(adminStaff), WorkType.CheckIn);

        await workRepository.AddAsync(work, trip, adminStaff, cancellationToken);
    }

    public async Task AddAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var work = GetNew(null, WorkType.Bay);

        await workRepository.AddAsync(work, trip, bay, cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, BayStaff bayStaff, Pellet pellet, WorkType workType, CancellationToken cancellationToken)
    {
        var work = GetNew(GetTime(bayStaff, pellet), workType);

        await workRepository.AddAsync(work, bay, bayStaff, pellet, cancellationToken);
    }

    public async Task AddAsync(Bay bay, Picker picker, Pellet pellet, CancellationToken cancellationToken)
    {
        var work = GetNew(GetTime(picker, pellet), WorkType.Fetch);

        await workRepository.AddAsync(work, bay, picker, pellet, cancellationToken);
    }
    
    public async Task AddAsync(Bay bay, Stuffer stuffer, Pellet pellet, CancellationToken cancellationToken)
    {
        var work = GetNew(GetTime(stuffer), WorkType.Stuff);

        await workRepository.AddAsync(work, bay, stuffer, pellet, cancellationToken);
    }
}