using Database;
using Database.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Services;
using Services.Abstractions;
using Services.AdminStaffServices;
using Services.BayServices;
using Services.BayStaffServices;
using Services.HubServices;
using Services.ModelServices;
using Services.ParkingSpotServices;
using Services.TripServices;
using Services.TruckCompanyServices;
using Services.TruckServices;

namespace Simulator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimulator(this IServiceCollection services)
    {
        services.AddLogging(configure =>
        {
            Log.Logger = new LoggerConfiguration()
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
                        LoadIds = b.Loads.Select(l => l.Id),
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
                        TruckCompanyStartId = l.TruckCompanyStartId,
                        HubId = l.HubId,
                        TruckCompanyEndId = l.TruckCompanyEndId,
                        DropOffTripId = l.DropOffTripId,
                        PickUpTripId = l.PickUpTripId,
                        BayId = l.BayId,
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
                .Destructure.ByTransforming<Trip>(
                    tp => new
                    {
                        Id = tp.Id,
                        XLocation = tp.XLocation,
                        YLocation = tp.YLocation,
                        DropOff = tp.DropOff,
                        PickUp = tp.PickUp,
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
                        YLocation = tc.YLocation
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
                        BayStaffId = w.BayStaffId
                    })
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            
            configure.ClearProviders();
            configure.AddSerilog(Log.Logger);
            // configure.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
        });

        services.AddDbContext<ModelDbContext>();

        services.AddScoped<HubService>();
        services.AddScoped<IPriorityInitializationService,  HubInitialize>();
        services.AddScoped<HubRepository>();

        services.AddScoped<TruckCompanyService>();
        services.AddScoped<IPriorityInitializationService,  TruckCompanyInitialize>();
        services.AddScoped<IStepperService,                 TruckCompanyStepper>();
        services.AddScoped<TruckCompanyRepository>();

        services.AddScoped<AdminStaffService>();
        services.AddScoped<IInitializationService,          AdminStaffInitialize>();
        services.AddScoped<IStepperService,                 AdminStaffStepper>();
        services.AddScoped<AdminStaffRepository>();

        services.AddScoped<BayService>();
        services.AddScoped<IInitializationService,          BayInitialize>();
        services.AddScoped<BayRepository>();

        services.AddScoped<BayStaffService>();
        services.AddScoped<IInitializationService,          BayStaffInitialize>();
        services.AddScoped<IStepperService,                 BayStaffStepper>();
        services.AddScoped<BayStaffRepository>();

        services.AddScoped<ParkingSpotService>();
        services.AddScoped<IInitializationService,          ParkingSpotInitialize>();
        services.AddScoped<IStepperService,                 ParkingSpotStepper>();
        services.AddScoped<ParkingSpotRepository>();

        services.AddScoped<TripService>();
        services.AddScoped<IStepperService,                 TripStepper>();
        services.AddScoped<TripRepository>();

        services.AddScoped<TruckService>();
        services.AddScoped<IInitializationService,          TruckInitialize>();
        services.AddScoped<IStepperService,                 TruckStepper>();
        services.AddScoped<TruckRepository>();

        services.AddScoped<AdminShiftService>();
        services.AddScoped<AdminShiftRepository>();

        services.AddScoped<BayShiftService>();
        services.AddScoped<BayShiftRepository>();

        services.AddScoped<LoadService>();
        services.AddScoped<LoadRepository>();

        services.AddScoped<OperatingHourService>();
        services.AddScoped<OperatingHourRepository>();

        services.AddScoped<WorkService>();
        services.AddScoped<WorkRepository>();

        services.AddScoped<LocationService>();

        services.AddScoped<ModelInitialize>();
        services.AddScoped<ModelStepper>();
        services.AddScoped<ModelState>();
        services.AddScoped<ModelService>();
        
        return services;
    }
}
