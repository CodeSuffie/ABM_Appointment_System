using System.Diagnostics.Metrics;
using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Repositories;
using Serilog;
using Services;
using Services.Abstractions;
using Services.Factories;
using Services.Initializers;
using Services.Steppers;
using Settings;

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
        
        // Repositories
        services.AddScoped<AdminStaffRepository>();
        services.AddScoped<AppointmentRepository>();
        services.AddScoped<AppointmentSlotRepository>();
        services.AddScoped<BayRepository>();
        services.AddScoped<BayShiftRepository>();
        services.AddScoped<BayStaffRepository>();
        services.AddScoped<HubRepository>();
        services.AddScoped<HubShiftRepository>();
        services.AddScoped<LoadRepository>();
        services.AddScoped<OperatingHourRepository>();
        services.AddScoped<ParkingSpotRepository>();
        services.AddScoped<PelletRepository>();
        services.AddScoped<PickerRepository>();
        services.AddScoped<StufferRepository>();
        services.AddScoped<TripRepository>();
        services.AddScoped<TruckCompanyRepository>();
        services.AddScoped<TruckRepository>();
        services.AddScoped<WarehouseRepository>();
        services.AddScoped<WorkRepository>();
        
        
        
        
        // Factories
        services.AddScoped<AdminShiftFactory>();
        services.AddScoped<AdminStaffFactory>();
        services.AddScoped<AppointmentFactory>();
        services.AddScoped<AppointmentSlotFactory>();
        services.AddScoped<BayFactory>();
        services.AddScoped<BayShiftFactory>();
        services.AddScoped<BayStaffFactory>();
        services.AddScoped<HubFactory>();
        services.AddScoped<LoadFactory>();
        services.AddScoped<OperatingHourFactory>();
        services.AddScoped<ParkingSpotFactory>();
        services.AddScoped<PelletFactory>();
        services.AddScoped<PickerFactory>();
        services.AddScoped<PickerShiftFactory>();
        services.AddScoped<StufferFactory>();
        services.AddScoped<StufferShiftFactory>();
        services.AddScoped<TripFactory>();
        services.AddScoped<TruckCompanyFactory>();
        services.AddScoped<TruckFactory>();
        services.AddScoped<WarehouseFactory>();
        services.AddScoped<WorkFactory>();
        services.AddScoped<LocationFactory>();
        

        // Services
        services.AddScoped<AdminStaffService>();
        services.AddScoped<BayService>();
        services.AddScoped<BayShiftService>();
        services.AddScoped<BayStaffService>();
        services.AddScoped<HubShiftService>();
        services.AddScoped<ParkingSpotService>();
        services.AddScoped<PelletService>();
        services.AddScoped<PickerService>();
        services.AddScoped<StufferService>();
        services.AddScoped<TripService>();
        services.AddScoped<TruckService>();
        services.AddScoped<WorkService>();
        
        
        
        // Initializers
        services.AddScoped<IPriorityInitializerService,     AdminStaffInitializer>();
        services.AddScoped<IPriorityInitializerService,     AppointmentInitializer>();
        services.AddScoped<IPriorityInitializerService,     AppointmentSlotInitializer>();
        services.AddScoped<IPriorityInitializerService,     BayInitializer>();
        services.AddScoped<IPriorityInitializerService,     BayStaffInitializer>();
        services.AddScoped<IPriorityInitializerService,     HubInitializer>();
        services.AddScoped<IPriorityInitializerService,     ParkingSpotInitializer>();
        services.AddScoped<IPriorityInitializerService,     PelletInitializer>();
        services.AddScoped<IPriorityInitializerService,     PickerInitializer>();
        services.AddScoped<IPriorityInitializerService,     StufferInitializer>();
        services.AddScoped<IPriorityInitializerService,     TruckCompanyInitializer>();
        services.AddScoped<IPriorityInitializerService,     TruckInitializer>();
        services.AddScoped<IPriorityInitializerService,     WarehouseInitializer>();
        
        
        
        // Steppers
        services.AddScoped<IStepperService,                 AdminStaffStepper>();
        services.AddScoped<IStepperService,                 AppointmentStepper>();
        services.AddScoped<IStepperService,                 AppointmentSlotStepper>();
        services.AddScoped<IStepperService,                 BayStepper>();
        services.AddScoped<IStepperService,                 BayStaffStepper>();
        services.AddScoped<IStepperService,                 HubStepper>();
        services.AddScoped<IStepperService,                 LoadStepper>();
        services.AddScoped<IStepperService,                 ParkingSpotStepper>();
        services.AddScoped<IStepperService,                 PelletStepper>();
        services.AddScoped<IStepperService,                 PickerStepper>();
        services.AddScoped<IStepperService,                 StufferStepper>();
        services.AddScoped<IStepperService,                 TripStepper>();
        services.AddScoped<IStepperService,                 TruckStepper>();
        

        services.AddScoped<ModelInitializer>();
        services.AddScoped<ModelStepper>();
        services.AddScoped<ModelState>();
        services.AddScoped<ModelService>();
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: Instrumentation.ServiceName, 
                serviceVersion: Instrumentation.ServiceVersion))
            .WithTracing(providerBuilder => providerBuilder
                .AddSource(Instrumentation.ServiceName)
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri("http://grafana-collector:4317/");
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                }))     // What is this for?
            .WithMetrics(providerBuilder => providerBuilder
                .AddMeter(Instrumentation.ServiceName)
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri("http://grafana-collector:4317/");
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                    exporter.ExportProcessorType = ExportProcessorType.Simple;
                    // exporter.BatchExportProcessorOptions.MaxExportBatchSize = 2048;
                    // exporter.BatchExportProcessorOptions.ExporterTimeoutMilliseconds = 10000;
                    // exporter.BatchExportProcessorOptions.MaxQueueSize = 8192;
                    // exporter.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1000;
                }));        // PrometheusExporter() ?

        services.AddSingleton<Instrumentation>();
        
        return services;
    }
}
