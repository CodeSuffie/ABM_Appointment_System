﻿using System.Diagnostics.Metrics;
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
using Services.AppointmentServices;
using Services.AppointmentSlotServices;
using Services.BayServices;
using Services.BayStaffServices;
using Services.HubServices;
using Services.LoadServices;
using Services.ModelServices;
using Services.ParkingSpotServices;
using Services.PelletServices;
using Services.PickerServices;
using Services.StufferServices;
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
        services.AddScoped<IPriorityInitializationService,  AdminStaffInitialize>();
        services.AddScoped<IStepperService,                 AdminStaffStepper>();
        services.AddScoped<AdminStaffRepository>();
        
        services.AddScoped<AppointmentService>();
        services.AddScoped<IPriorityInitializationService,  AppointmentInitialize>();
        services.AddScoped<IStepperService,                 AppointmentStepper>();
        services.AddScoped<AppointmentRepository>();
        
        services.AddScoped<AppointmentSlotService>();
        services.AddScoped<IPriorityInitializationService,  AppointmentSlotInitialize>();
        services.AddScoped<IStepperService,                 AppointmentSlotStepper>();
        services.AddScoped<AppointmentSlotRepository>();

        services.AddScoped<BayService>();
        services.AddScoped<IPriorityInitializationService,  BayInitialize>();
        services.AddScoped<IStepperService,                 BayStepper>();
        services.AddScoped<BayRepository>();

        services.AddScoped<BayStaffService>();
        services.AddScoped<IPriorityInitializationService,  BayStaffInitialize>();
        services.AddScoped<IStepperService,                 BayStaffStepper>();
        services.AddScoped<BayStaffRepository>();

        services.AddScoped<ParkingSpotService>();
        services.AddScoped<IPriorityInitializationService,  ParkingSpotInitialize>();
        services.AddScoped<IStepperService,                 ParkingSpotStepper>();
        services.AddScoped<ParkingSpotRepository>();
        
        services.AddScoped<PelletFactory>();
        services.AddScoped<PelletService>();
        services.AddScoped<IPriorityInitializationService,  PelletInitialize>();
        services.AddScoped<IStepperService,                 PelletStepper>();
        services.AddScoped<PelletRepository>();
        
        services.AddScoped<PickerService>();
        services.AddScoped<IPriorityInitializationService,  PickerInitialize>();
        services.AddScoped<IStepperService,                 PickerStepper>();
        services.AddScoped<PickerRepository>();
        
        services.AddScoped<StufferService>();
        services.AddScoped<IPriorityInitializationService,  StufferInitialize>();
        services.AddScoped<IStepperService,                 StufferStepper>();
        services.AddScoped<StufferRepository>();
        
        services.AddScoped<TripService>();
        services.AddScoped<IStepperService,                 TripStepper>();
        services.AddScoped<TripRepository>();

        services.AddScoped<TruckService>();
        services.AddScoped<IPriorityInitializationService,  TruckInitialize>();
        services.AddScoped<IStepperService,                 TruckStepper>();
        services.AddScoped<TruckRepository>();
        
        services.AddScoped<WarehouseService>();
        services.AddScoped<IPriorityInitializationService,  WarehouseInitialize>();
        services.AddScoped<WarehouseRepository>();

        services.AddScoped<LoadService>();
        services.AddScoped<IStepperService,                 LoadStepper>();
        services.AddScoped<LoadRepository>();

        services.AddScoped<HubShiftService>();
        services.AddScoped<HubShiftRepository>();

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
