using System.Diagnostics.Metrics;
using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Repositories;
using Serilog;
using Services;
using Services.Abstractions;
using Services.AdminStaffServices;
using Services.BayServices;
using Services.BayStaffServices;
using Services.HubServices;
using Services.LoadServices;
using Services.ModelServices;
using Services.ParkingSpotServices;
using Services.PelletServices;
using Services.TripServices;
using Services.TruckCompanyServices;
using Services.TruckServices;
using Services.WarehouseServices;

namespace Simulator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSimulator(
        this IServiceCollection services //,
        //IMeterFactory meterFactory
        )
    {
        services.AddLogging(configure =>
        {
            Log.Logger = LoggerFactory.CreateLogger();
            
            configure.ClearProviders();
            configure.AddSerilog(Log.Logger);
            // configure.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
        });

        services.AddDbContext<ModelDbContext>();

        services.AddScoped<HubService>();
        services.AddScoped<IPriorityInitializationService,  HubInitialize>();
        services.AddScoped<IStepperService,                 HubStepper>();
        services.AddScoped<HubRepository>();

        services.AddScoped<TruckCompanyService>();
        services.AddScoped<IPriorityInitializationService,  TruckCompanyInitialize>();
        services.AddScoped<TruckCompanyRepository>();

        services.AddScoped<AdminStaffService>();
        services.AddScoped<IInitializationService,          AdminStaffInitialize>();
        services.AddScoped<IStepperService,                 AdminStaffStepper>();
        services.AddScoped<AdminStaffRepository>();

        services.AddScoped<BayService>();
        services.AddScoped<IInitializationService,          BayInitialize>();
        services.AddScoped<IStepperService,                 BayStepper>();
        services.AddScoped<BayRepository>();

        services.AddScoped<BayStaffService>();
        services.AddScoped<IInitializationService,          BayStaffInitialize>();
        services.AddScoped<IStepperService,                 BayStaffStepper>();
        services.AddScoped<BayStaffRepository>();

        services.AddScoped<ParkingSpotService>();
        services.AddScoped<IInitializationService,          ParkingSpotInitialize>();
        services.AddScoped<IStepperService,                 ParkingSpotStepper>();
        services.AddScoped<ParkingSpotRepository>();
        
        services.AddScoped<PelletCreation>();
        services.AddScoped<PelletService>();
        services.AddScoped<IInitializationService,          PelletInitialize>();
        services.AddScoped<IStepperService,                 PelletStepper>();
        services.AddScoped<PelletRepository>();

        services.AddScoped<TripService>();
        services.AddScoped<IStepperService,                 TripStepper>();
        services.AddScoped<TripRepository>();

        services.AddScoped<TruckService>();
        services.AddScoped<IInitializationService,          TruckInitialize>();
        services.AddScoped<IStepperService,                 TruckStepper>();
        services.AddScoped<TruckRepository>();
        
        services.AddScoped<WarehouseService>();
        services.AddScoped<IInitializationService,          WarehouseInitialize>();
        services.AddScoped<WarehouseRepository>();

        services.AddScoped<LoadService>();
        services.AddScoped<IStepperService,                 LoadStepper>();
        services.AddScoped<LoadRepository>();

        services.AddScoped<AdminShiftService>();
        services.AddScoped<AdminShiftRepository>();

        services.AddScoped<BayShiftService>();
        services.AddScoped<BayShiftRepository>();

        services.AddScoped<OperatingHourService>();
        services.AddScoped<OperatingHourRepository>();

        services.AddScoped<WorkService>();
        services.AddScoped<WorkRepository>();

        services.AddScoped<LocationService>();

        services.AddScoped<ModelInitialize>();
        services.AddScoped<ModelStepper>();
        services.AddScoped<ModelState>();
        services.AddScoped<ModelService>();
        
        var serviceName = "Simulator";  // Maybe Apps.ConsoleApp & Apps.DesktopApp?
        var serviceVersion = "1.0.0";
        var serviceEnvironment = "";
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName, 
                serviceVersion: serviceVersion))
            .WithTracing(providerBuilder => providerBuilder
                .AddSource(serviceName)
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri("http://grafana-collector:4317/");
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                }))     // What is this for?
            .WithMetrics(providerBuilder => providerBuilder
                .AddMeter(serviceName)
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri("http://grafana-collector:4317/");
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                }));        // PrometheusExporter() ?
        
        var meter = new Meter(serviceName, serviceVersion);
        // var meter = meterFactory.Create(serviceName, serviceVersion); ?
        services.AddSingleton(meter);
        
        return services;
    }
}
