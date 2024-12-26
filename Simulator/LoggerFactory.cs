using Database.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

// ReSharper disable All

namespace Simulator;

internal static class LoggerFactory
{
    public static Logger CreateLogger()
    {
        return new LoggerConfiguration()
                .Destructure.ByTransforming<AdminStaff>(
                    ads => new 
                    { 
                        Id = ads.Id,
                        HubId = ads.HubId,
                        Work = ads.Work,
                        TripId = ads.TripId,
                        WorkChance = ads.WorkChance,
                        Speed = ads.Speed,
                        AverageShiftLength = ads.AverageShiftLength,
                        ShiftIds = ads.Shifts.Select(sh => sh.Id),
                    })
                .Destructure.ByTransforming<Appointment>(
                    ap => new
                    {
                        Id = ap.Id,
                        AppointmentSlotId = ap.AppointmentSlotId,
                        TripId = ap.TripId,
                        BayId = ap.BayId,
                    })
                .Destructure.ByTransforming<AppointmentSlot>(
                    aps => new
                    {
                        Id = aps.Id,
                        StartTime = aps.StartTime,
                        HubId = aps.HubId,
                        AppointmentIds = aps.Appointments.Select(ap => ap.Id),
                    })
                .Destructure.ByTransforming<Bay>(
                    b => new
                    {
                        Id = b.Id,
                        Capacity = b.Capacity,
                        BayStatus = b.BayStatus,
                        BayFlags = b.BayFlags,
                        HubId = b.HubId,
                        TripId = b.TripId,
                        InventoryIds = b.Inventory.Select(l => l.Id),
                        WorkIds = b.Works.Select(w => w.Id),
                        AppointmentIds = b.Appointments.Select(ap => ap.Id),
                    })
                .Destructure.ByTransforming<BayShift>(
                    bsh => new 
                    { 
                        Id = bsh.Id,
                        StartTime = bsh.StartTime,
                        Duration = bsh.Duration,
                        BayId = bsh.BayId,
                        BayStaffId = bsh.BayStaffId,
                    })
                .Destructure.ByTransforming<BayStaff>(
                    bs => new
                    {
                        Id = bs.Id,
                        HubId = bs.HubId,
                        Work = bs.Work,
                        WorkChance = bs.WorkChance,
                        Speed = bs.Speed,
                        AverageShiftLength = bs.AverageShiftLength,
                        ShiftIds = bs.Shifts.Select(sh => sh.Id)
                    })
                .Destructure.ByTransforming<Hub>(
                    h => new
                    {
                        Id = h.Id,
                        WorkChance = h.WorkChance,
                        AverageShiftLength = h.AverageShiftLength,
                        AdminStaffIds = h.AdminStaffs.Select(ads => ads.Id),
                        BayStaffIds = h.BayStaffs.Select(bs => bs.Id),
                        PickerIds = h.Pickers.Select(p => p.Id),
                        StufferIds = h.Stuffers.Select(s => s.Id),
                        Warehouse = h.Warehouse,
                        ParkingSpotIds = h.ParkingSpots.Select(ps => ps.Id),
                        BayIds = h.Bays.Select(b => b.Id),
                        TripIds = h.Trips.Select(tp => tp.Id),
                        AppointmentSlotIds = h.AppointmentSlots.Select(aps => aps.Id),
                        ShiftIds = h.Shifts.Select(h => h.Id)
                    })
                .Destructure.ByTransforming<HubShift>(
                    adsh => new 
                    {
                        Id = adsh.Id,
                        StartTime = adsh.StartTime,
                        Duration = adsh.Duration,
                        AdminStaffId = adsh.AdminStaffId,
                        PickerId = adsh.PickerId,
                        StufferId = adsh.StufferId,
                    })
                .Destructure.ByTransforming<Load>(
                    l => new
                    {
                        Id = l.Id,
                        LoadType = l.LoadType,
                        TruckCompanyId = l.TruckCompanyId,
                        HubId = l.HubId,
                        TripId = l.TripId,
                        PelletIds = l.Pellets.Select(p => p.Id)
                    })
                .Destructure.ByTransforming<OperatingHour>(
                    oh => new { 
                        Id = oh.Id,
                        StartTime = oh.StartTime,
                        Duration = oh.Duration,
                        HubId = oh.HubId 
                    })
                .Destructure.ByTransforming<ParkingSpot>(
                    ps => new
                    {
                        Id = ps.Id,
                        HubId = ps.HubId,
                        TripId = ps.TripId,
                    })
                .Destructure.ByTransforming<Pellet>(
                    p => new
                    {
                        Id = p.Id,
                        Difficulty = p.Difficulty,
                        TruckCompanyId = p.TruckCompanyId,
                        TruckId = p.TruckId,
                        BayId = p.BayId,
                        WarehouseId = p.WarehouseId,
                        LoadId = p.LoadId,
                        WorkId = p.WorkId
                    })
                .Destructure.ByTransforming<Picker>(
                    pi => new
                    {
                        Id = pi.Id,
                        HubId = pi.HubId,
                        Work = pi.Work,
                        WorkChance = pi.WorkChance,
                        Speed = pi.Speed,
                        Experience = pi.Experience,
                        AverageShiftLength = pi.AverageShiftLength,
                        ShiftIds = pi.Shifts.Select(sh => sh.Id)
                    })
                .Destructure.ByTransforming<Stuffer>(
                    s => new
                    {
                        Id = s.Id,
                        HubId = s.HubId,
                        Work = s.Work,
                        WorkChance = s.WorkChance,
                        Speed = s.Speed,
                        Experience = s.Experience,
                        AverageShiftLength = s.AverageShiftLength,
                        ShiftIds = s.Shifts.Select(sh => sh.Id)
                    })
                .Destructure.ByTransforming<Trip>(
                    tp => new
                    {
                        Id = tp.Id,
                        XLocation = tp.XLocation,
                        YLocation = tp.YLocation,
                        Completed = tp.Completed,
                        LoadIds = tp.Loads.Select(l => l.Id),
                        Truck = tp.Truck,
                        Hub = tp.Hub,
                        ParkingSpot = tp.ParkingSpot,
                        AdminStaff = tp.AdminStaff,
                        Bay = tp.Bay,
                        Work = tp.Work,
                        Appointment = tp.Appointment,
                        TravelTime = tp.TravelTime
                    })
                .Destructure.ByTransforming<Truck>(
                    tk => new
                    {
                        Id = tk.Id,
                        Capacity = tk.Capacity,
                        Speed = tk.Speed,
                        TruckCompanyId = tk.TruckCompanyId,
                        TripId = tk.TripId,
                        InventoryIds = tk.Inventory.Select(p => p.Id)
                    })
                .Destructure.ByTransforming<TruckCompany>(
                    tc => new
                    {
                        Id = tc.Id,
                        Capacity = tc.Capacity,
                        TruckIds = tc.Trucks.Select(tk => tk.Id),
                        InventoryIds = tc.Inventory.Select(p => p.Id),
                    })
                .Destructure.ByTransforming<Warehouse>(
                    w => new
                    {
                        Id = w.Id,
                        Capacity = w.Capacity,
                        HubId = w.HubId,
                        InventoryIds = w.Inventory.Select(p => p.Id),
                    })
                .Destructure.ByTransforming<Work>(
                    w => new
                    {
                        Id = w.Id,
                        StartTime = w.StartTime,
                        Duration = w.Duration,
                        WorkType = w.WorkType,
                        TripId = w.TripId,
                        AdminStaffId = w.AdminStaffId,
                        BayStaffId = w.BayStaffId,
                        PickerId = w.PickerId,
                        StufferId = w.StufferId,
                        BayId = w.BayId,
                        PelletId = w.PelletId
                    })
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
    }
}