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
                        AverageShiftLength = ads.AverageShiftLength,
                        ShiftIds = ads.Shifts.Select(sh => sh.Id),
                    })
                .Destructure.ByTransforming<Bay>(
                    b => new
                    {
                        Id = b.Id,
                        BayStatus = b.BayStatus,
                        BayFlags = b.BayFlags,
                        HubId = b.HubId,
                        TripId = b.TripId,
                        InventoryIds = b.Inventory.Select(l => l.Id),
                        WorkIds = b.Works.Select(w => w.Id),
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
                        AverageShiftLength = bs.AverageShiftLength,
                        ShiftIds = bs.Shifts.Select(sh => sh.Id)
                    })
                .Destructure.ByTransforming<Hub>(
                    h => new
                    {
                        Id = h.Id,
                        OperatingChance = h.WorkChance,
                        AverageOperatingHourLength = h.AverageShiftLength,
                        AdminStaffIds = h.AdminStaffs.Select(ads => ads.Id),
                        BayStaffIds = h.BayStaffs.Select(bs => bs.Id),
                        WarehouseId = h.WarehouseId,
                        ParkingSpotIds = h.ParkingSpots.Select(ps => ps.Id),
                        BayIds = h.Bays.Select(b => b.Id),
                        TripIds = h.Trips.Select(tp => tp.Id),
                        OperatingHourIds = h.Shifts.Select(oh => oh.Id),
                        // XSize = h.XSize,
                        // YSize = h.YSize,
                        // XLocation = h.XLocation,
                        // YLocation = h.YLocation
                    })
                .Destructure.ByTransforming<HubShift>(
                    adsh => new 
                    {
                        Id = adsh.Id,
                        StartTime = adsh.StartTime,
                        Duration = adsh.Duration,
                        AdminStaffId = adsh.AdminStaffId,
                        PickerId = adsh.AdminStaffId,
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
                        TruckCompanyId = p.TruckCompanyId,
                        LoadId = p.LoadId,
                        BayId = p.BayId,
                        WarehouseId = p.WarehouseId,
                        WorkId = p.WorkId
                    })
                .Destructure.ByTransforming<Picker>(
                    pi => new
                    {
                        Id = pi.Id,
                        HubId = pi.HubId,
                        Work = pi.Work,
                        WorkChance = pi.WorkChance,
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
                        Work = tp.Work
                    })
                .Destructure.ByTransforming<Truck>(
                    tk => new
                    {
                        Id = tk.Id,
                        Capacity = tk.Capacity,
                        Speed = tk.Speed,
                        TruckCompanyId = tk.TruckCompanyId,
                        TripId = tk.TripId
                    })
                .Destructure.ByTransforming<TruckCompany>(
                    tc => new
                    {
                        Id = tc.Id,
                        TruckIds = tc.Trucks.Select(tk => tk.Id),
                        InventoryIds = tc.Inventory.Select(p => p.Id),
                    })
                .Destructure.ByTransforming<Warehouse>(
                    w => new
                    {
                        Id = w.Id,
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
                        BayId = w.BayId,
                        PelletId = w.PelletId
                    })
                .MinimumLevel.Error()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
    }
}