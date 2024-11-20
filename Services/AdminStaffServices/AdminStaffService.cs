using Database.Models;
using Repositories;
using Services.HubServices;
using Services.TripServices;
using Settings;

namespace Services.AdminStaffServices;

public sealed class AdminStaffService(
    HubService hubService,
    HubRepository hubRepository,
    TripRepository tripRepository,
    TripService tripService,
    WorkRepository workRepository)
{
    public async Task<AdminStaff> GetNewObjectAsync(CancellationToken cancellationToken)
    {
        var hub = await hubService.SelectHubAsync(cancellationToken);
        
        var adminStaff = new AdminStaff
        {
            Hub = hub,
            WorkChance = AgentConfig.AdminStaffAverageWorkDays,
            AverageShiftLength = AgentConfig.AdminShiftAverageLength
        };

        return adminStaff;
    }
    
    public async Task<double> GetWorkChanceAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        
        return adminStaff.WorkChance / hub.OperatingChance;
    }
    
    public async Task AlertClaimedAsync(AdminStaff adminStaff, Trip trip, CancellationToken cancellationToken)
    {
        await tripRepository.SetAsync(trip, adminStaff, cancellationToken);
    }
    
    public async Task AlertUnclaimedAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(adminStaff, cancellationToken);
        if (trip == null)
            throw new Exception("This AdminStaff was just told to be unclaimed but no Trip was assigned");

        await tripRepository.UnsetAsync(trip, adminStaff, cancellationToken);
    }

    public async Task AlertWorkCompleteAsync(AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetAsync(adminStaff, cancellationToken);
        if (trip == null) 
            throw new Exception("This AdminStaff was just told to have completed a Check In but no Trip is assigned");
        
        await tripService.AlertCheckInCompleteAsync(trip, cancellationToken);
        
        var work = await workRepository.GetAsync(adminStaff, cancellationToken);
        if (work == null) return;
        await workRepository.RemoveAsync(work, cancellationToken);
    }
    
    public async Task AlertFreeAsync(AdminStaff adminStaff, CancellationToken cancellationToken) 
    {
        var hub = await hubRepository.GetAsync(adminStaff, cancellationToken);
        
        
         
        var trip = await tripService.GetNextAsync(hub, WorkType.WaitCheckIn, cancellationToken);
        if (trip == null) return;      // TODO: Log no waiting Trips

        await tripService.AlertFreeAsync(trip, adminStaff, cancellationToken);
    }
}
