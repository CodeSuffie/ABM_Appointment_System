using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.AdminStaffServices;
using Services.BayServices;
using Services.ParkingSpotServices;

namespace Services.TripServices;

public sealed class TripService(
    LoadService loadService,
    WorkRepository workRepository,
    ParkingSpotService parkingSpotService,
    ParkingSpotRepository parkingSpotRepository,
    AdminStaffRepository adminStaffRepository,
    AdminStaffService adminStaffService,
    TripRepository tripRepository,
    HubRepository hubRepository,
    BayService bayService,
    BayRepository bayRepository)
{
    public async Task<Trip?> GetNewObjectAsync(TruckCompany truckCompany, CancellationToken cancellationToken)
    {
        var dropOff = await loadService.SelectUnclaimedDropOffAsync(truckCompany, cancellationToken);
        Load? pickUp = null;

        if (dropOff == null)
        {
            pickUp = await loadService.SelectUnclaimedPickUpAsync(cancellationToken);
        }
        else
        {
            var hub = await hubRepository.GetAsync(dropOff, cancellationToken);
            if (hub == null) throw new Exception("DropOff Load was not matched on a valid Hub.");

            pickUp = await loadService.SelectUnclaimedPickUpAsync(hub, cancellationToken);
        }

        var trip = new Trip
        {
            DropOff = dropOff,
            PickUp = pickUp,
        };

        return trip;
    }
    
    public async Task AlertFreeAsync(Trip trip, ParkingSpot parkingSpot, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitParking })
        {
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await parkingSpotService.AlertClaimedAsync(parkingSpot, trip, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitCheckIn, cancellationToken);

        }
    }

    public async Task AlertFreeAsync(Trip trip, AdminStaff adminStaff, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitCheckIn })
        {
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await adminStaffService.AlertClaimedAsync(adminStaff, trip, cancellationToken);
            await workRepository.AddAsync(trip, adminStaff, cancellationToken);
        }
    }

    public async Task AlertFreeAsync(Trip trip, Bay bay, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.WaitBay })
        {
            var parkingSpot = await parkingSpotRepository.GetAsync(trip, cancellationToken);
            if (parkingSpot != null)
            {
                await parkingSpotService.AlertUnclaimedAsync(parkingSpot, cancellationToken);
            }

            await workRepository.RemoveAsync(oldWork, cancellationToken);
            await bayService.AlertClaimedAsync(bay, trip, cancellationToken);
            await workRepository.AddAsync(trip, bay, cancellationToken);
        }
    }
    
    public async Task AlertCheckInCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.CheckIn })
        {
            var adminStaff = await adminStaffRepository.GetAsync(trip, cancellationToken);
            if (adminStaff == null)
                throw new Exception ("The CheckIn for this Trip has just completed but there was no AdminStaff assigned");
            
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await adminStaffService.AlertUnclaimedAsync(adminStaff, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.WaitBay, cancellationToken);
        }
    }
    
    public async Task AlertBayWorkCompleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        var oldWork = await workRepository.GetAsync(trip, cancellationToken);
        if (oldWork is { WorkType: WorkType.Bay })
        {
            var bay = await bayRepository.GetAsync(trip, cancellationToken);
            if (bay == null)
                throw new Exception ("The Bay Work for this Trip has just completed but there was no Bay assigned");
            
            await workRepository.RemoveAsync(oldWork, cancellationToken);
                await bayService.AlertUnclaimedAsync(bay, cancellationToken);
            await workRepository.AddAsync(trip, WorkType.TravelHome, cancellationToken);
        }
    }

    public async Task<Trip?> GetNextAsync(Hub hub, WorkType workType, CancellationToken cancellationToken)
    {
        var trips = (await tripRepository.GetCurrentAsync(hub, workType, cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        Trip? nextTrip = null;
        TimeSpan? earliestStart = null;
        await foreach (var trip in trips)
        {
            var work = await workRepository.GetAsync(trip, cancellationToken);
            if (nextTrip != null && (work == null ||
                                     (work.StartTime > earliestStart))) continue;
            nextTrip = trip;
            earliestStart = work?.StartTime;
        }

        return nextTrip;
    }
}
