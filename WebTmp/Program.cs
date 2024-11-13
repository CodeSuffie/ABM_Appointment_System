using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Abstractions;
using Services;
using Services.AdminStaffServices;
using Services.BayServices;
using Services.BayStaffServices;
using Services.HubServices;
using Services.ModelServices;
using Services.ParkingSpotServices;
using Services.TripServices;
using Services.TruckCompanyServices;
using Services.TruckServices;

var hostBuilder = new HostApplicationBuilder(args);
hostBuilder.Services.AddDbContext<ModelDbContext>();

hostBuilder.Services.AddScoped<HubService>();
hostBuilder.Services.AddScoped<IInitializationService, HubInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        HubStepper>();

hostBuilder.Services.AddScoped<TruckCompanyService>();
hostBuilder.Services.AddScoped<IInitializationService, TruckCompanyInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        TruckCompanyStepper>();

hostBuilder.Services.AddScoped<AdminStaffService>();
hostBuilder.Services.AddScoped<IInitializationService, AdminStaffInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        AdminStaffStepper>();

hostBuilder.Services.AddScoped<BayService>();
hostBuilder.Services.AddScoped<IInitializationService, BayInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        BayStepper>();

hostBuilder.Services.AddScoped<BayStaffService>();
hostBuilder.Services.AddScoped<IInitializationService, BayStaffInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        BayStaffStepper>();

hostBuilder.Services.AddScoped<ParkingSpotService>();
hostBuilder.Services.AddScoped<IInitializationService, ParkingSpotInitialize>();

hostBuilder.Services.AddScoped<TripService>();
hostBuilder.Services.AddScoped<IStepperService,        TripStepper>();

hostBuilder.Services.AddScoped<TruckService>();
hostBuilder.Services.AddScoped<IInitializationService, TruckInitialize>();
hostBuilder.Services.AddScoped<IStepperService,        TruckStepper>();

hostBuilder.Services.AddScoped<AdminShiftService>();
hostBuilder.Services.AddScoped<BayShiftService>();
hostBuilder.Services.AddScoped<LoadService>();
hostBuilder.Services.AddScoped<LocationService>();
hostBuilder.Services.AddScoped<OperatingHourService>();
hostBuilder.Services.AddScoped<WorkService>();


hostBuilder.Services.AddScoped<ModelInitialize>();
hostBuilder.Services.AddScoped<ModelStepper>();
hostBuilder.Services.AddHostedService<ModelService>();


var host = hostBuilder.Build();
await host.RunAsync();
