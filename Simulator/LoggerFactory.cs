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
                .Destructure.ByTransforming<AdminShift>(
                    adsh => new 
                    {
                        Id = adsh.Id,
                        StartTime = adsh.StartTime,
                        Duration = adsh.Duration,
                        AdminStaffId = adsh.AdminStaffId
                    })
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
                        HubId = b.HubId,
                        TripId = b.TripId,
                        PelletIds = b.Pellets.Select(l => l.Id),
                        WorkIds = b.Works.Select(w => w.Id),
                        XSize = b.XSize,
                        YSize = b.YSize,
                        XLocation = b.XLocation,
                        YLocation = b.YLocation,
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
                        OperatingChance = h.OperatingChance,
                        AverageOperatingHourLength = h.AverageOperatingHourLength,
                        ParkingSpotIds = h.ParkingSpots.Select(ps => ps.Id),
                        AdminStaffIds = h.AdminStaffs.Select(ads => ads.Id),
                        BayStaffIds = h.BayStaffs.Select(bs => bs.Id),
                        BayIds = h.Bays.Select(b => b.Id),
                        TripIds = h.Trips.Select(tp => tp.Id),
                        OperatingHourIds = h.OperatingHours.Select(oh => oh.Id),
                        XSize = h.XSize,
                        YSize = h.YSize,
                        XLocation = h.XLocation,
                        YLocation = h.YLocation
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
                        XSize = ps.XSize,
                        YSize = ps.YSize,
                        XLocation = ps.XLocation,
                        YLocation = ps.YLocation
                    })
                .Destructure.ByTransforming<Pellet>(
                    p => new
                    {
                        Id = p.Id,
                        TruckCompanyId = p.TruckCompanyId,
                        LoadId = p.LoadId,
                        BayId = p.BayId,
                    })
                .Destructure.ByTransforming<Trip>(
                    tp => new
                    {
                        Id = tp.Id,
                        XLocation = tp.XLocation,
                        YLocation = tp.YLocation,
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
                        Speed = tk.Speed,
                        TruckCompanyId = tk.TruckCompanyId,
                        TripId = tk.TripId
                    })
                .Destructure.ByTransforming<TruckCompany>(
                    tc => new
                    {
                        Id = tc.Id,
                        TruckIds = tc.Trucks.Select(tk => tk.Id),
                        XSize = tc.XSize,
                        YSize = tc.YSize,
                        XLocation = tc.XLocation,
                        YLocation = tc.YLocation,
                        PelletIds = tc.Pellets.Select(p => p.Id),
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
                        BayId = w.BayId,
                        BayStaffId = w.BayStaffId,
                        PelletId = w.PelletId
                    })
                .MinimumLevel.Error()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
    }
}