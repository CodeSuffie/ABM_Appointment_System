using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories;
using Serilog;
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
            configure.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            
            configure.ClearProviders();
            configure.AddSerilog(Log.Logger);
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
