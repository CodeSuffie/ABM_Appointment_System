using System.Runtime.CompilerServices;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services;

public sealed class StufferService(
    ILogger<StufferService> logger,
    WorkRepository workRepository,
    WorkFactory workFactory,
    PelletRepository pelletRepository,
    BayRepository bayRepository,
    HubRepository hubRepository,
    PelletService pelletService,
    AppointmentSlotRepository appointmentSlotRepository,
    AppointmentRepository appointmentRepository,
    ModelState modelState)
{
    public async Task AlertWorkCompleteAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        var work = await workRepository.GetAsync(stuffer, cancellationToken);
        if (work == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have Work assigned to alert completed for.",
                stuffer);

            return;
        }

        var pellet = await pelletRepository.GetAsync(work, cancellationToken);
        if (pellet == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a Pellet assigned to Stuff.",
                stuffer,
                work);

            return;
        }
        
        var bay = await bayRepository.GetAsync(work, cancellationToken);
        if (bay == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n its assigned Work \n({@Work})\n did not have a bay assigned to Stuff the Pellet \n({@Pellet})\n for.",
                stuffer,
                work,
                pellet);

            return;
        }
        
        await pelletService.AlertStuffedAsync(pellet, bay, cancellationToken);
    }
    
    public async Task AlertFreeAppointmentAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        if (!modelState.ModelConfig.AppointmentSystemMode)
        {
            logger.LogError("This function cannot be called without Appointment System Mode.");

            return;
        }
        
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.",
                stuffer);

            return;
        }

        var bays = bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var appointmentSlots = appointmentSlotRepository.GetAfter(hub,
                modelState.ModelTime -
                modelState.AppointmentConfig!.AppointmentLength * modelState.ModelConfig.ModelStep)
            .Where(aps => aps.Appointments.Count != 0)
            .OrderBy(aps => aps.StartTime)
            .Take((modelState.AppointmentConfig!.AppointmentLength / modelState.AppointmentConfig!.AppointmentSlotDifference) + 1);
        
        Bay? bestBay = null;
        var stuffPelletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPelletCount = (await pelletService
                    .GetAvailableStuffPelletsAsync(bay, appointmentSlots, cancellationToken))
                .Count;
            
            if (bestBay != null && bayStuffPelletCount <= stuffPelletCount)
            {
                continue;
            }
            
            stuffPelletCount = bayStuffPelletCount;
            bestBay = bay;
        }

        if (bestBay != null)
        {
            await StartStuffAsync(stuffer, bestBay, appointmentSlots, cancellationToken);
        }
    }
    public async Task AlertFreeAsync(Stuffer stuffer, CancellationToken cancellationToken)
    {
        if (modelState.ModelConfig.AppointmentSystemMode)
        {
            await AlertFreeAppointmentAsync(stuffer, cancellationToken);
            
            return;
        }
        
        var hub = await hubRepository.GetAsync(stuffer, cancellationToken);
        if (hub == null)
        {
            logger.LogError("Stuffer \n({@Stuffer})\n did not have a Hub assigned to alert free for.",
                stuffer);

            return;
        }

        var bays = bayRepository.Get(hub)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Bay? bestBay = null;
        var stuffPelletCount = 0;
        await foreach (var bay in bays)
        {
            var bayStuffPelletCount = (await pelletService
                    .GetAvailableStuffPelletsAsync(bay, cancellationToken))
                .Count;

            if (bestBay != null && bayStuffPelletCount <= stuffPelletCount)
            {
                continue;
            }
            
            stuffPelletCount = bayStuffPelletCount;
            bestBay = bay;
        }

        if (bestBay == null)
        {
            logger.LogInformation("Stuffer \n({@Stuffer})\n its assigned Hub \n({@Hub})\n did not have a " +
                                   "Bay with more Pellets assigned to Stuff.",
                stuffer,
                hub);
            
            logger.LogDebug("Stuffer \n({@Stuffer})\n will remain idle...",
                stuffer);

            return;
        }

        await StartStuffAsync(stuffer, bestBay, cancellationToken);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, CancellationToken cancellationToken)
    {
        var pellet = await pelletService.GetNextStuffAsync(bay, cancellationToken);
        if (pellet == null)
        {
            logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Stuff.",
                bay);
            
            logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pellet \n({@Pellet})",
            stuffer,
            bay,
            pellet);
        await workFactory.GetNewObjectAsync(bay, stuffer, pellet, cancellationToken);
    }
    
    private async Task StartStuffAsync(Stuffer stuffer, Bay bay, IQueryable<AppointmentSlot> appointmentSlots, CancellationToken cancellationToken)
    {
        var pellet = await pelletService.GetNextStuffAsync(bay, appointmentSlots, cancellationToken);
        if (pellet == null)
        {
            logger.LogInformation("Bay \n({@Bay})\n did not have any more Pellets assigned to Stuff.",
                bay);
            
            logger.LogInformation("Stuff Work could not be started for this Bay \n({@Bay}).",
                bay);
            
            return;
        }
        
        logger.LogDebug("Adding Work for this Stuffer \n({@Stuffer})\n at this Bay \n({@Bay}) to Stuff this Pellet \n({@Pellet})",
            stuffer,
            bay,
            pellet);
        await workFactory.GetNewObjectAsync(bay, stuffer, pellet, cancellationToken);
    }

}