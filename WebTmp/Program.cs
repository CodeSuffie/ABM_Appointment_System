using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories;
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
hostBuilder.Services.AddScoped<IPriorityInitializationService,  HubInitialize>();
hostBuilder.Services.AddScoped<HubRepository>();
hostBuilder.Services.AddScoped<HubLogger>();

hostBuilder.Services.AddScoped<TruckCompanyService>();
hostBuilder.Services.AddScoped<IPriorityInitializationService,  TruckCompanyInitialize>();
hostBuilder.Services.AddScoped<IStepperService,                 TruckCompanyStepper>();
hostBuilder.Services.AddScoped<TruckCompanyRepository>();

hostBuilder.Services.AddScoped<AdminStaffService>();
hostBuilder.Services.AddScoped<IInitializationService,          AdminStaffInitialize>();
hostBuilder.Services.AddScoped<IStepperService,                 AdminStaffStepper>();
hostBuilder.Services.AddScoped<AdminStaffRepository>();
hostBuilder.Services.AddScoped<AdminStaffLogger>();

hostBuilder.Services.AddScoped<BayService>();
hostBuilder.Services.AddScoped<IInitializationService,          BayInitialize>();
hostBuilder.Services.AddScoped<BayRepository>();
hostBuilder.Services.AddScoped<BayLogger>();

hostBuilder.Services.AddScoped<BayStaffService>();
hostBuilder.Services.AddScoped<IInitializationService,          BayStaffInitialize>();
hostBuilder.Services.AddScoped<IStepperService,                 BayStaffStepper>();
hostBuilder.Services.AddScoped<BayStaffRepository>();
hostBuilder.Services.AddScoped<BayStaffLogger>();

hostBuilder.Services.AddScoped<ParkingSpotService>();
hostBuilder.Services.AddScoped<IInitializationService,          ParkingSpotInitialize>();
hostBuilder.Services.AddScoped<ParkingSpotRepository>();

hostBuilder.Services.AddScoped<TripService>();
hostBuilder.Services.AddScoped<IStepperService,                 TripStepper>();
hostBuilder.Services.AddScoped<TripRepository>();
hostBuilder.Services.AddScoped<TripLogger>();

hostBuilder.Services.AddScoped<TruckService>();
hostBuilder.Services.AddScoped<IInitializationService,          TruckInitialize>();
hostBuilder.Services.AddScoped<IStepperService,                 TruckStepper>();
hostBuilder.Services.AddScoped<TruckRepository>();

hostBuilder.Services.AddScoped<AdminShiftService>();
hostBuilder.Services.AddScoped<AdminShiftRepository>();

hostBuilder.Services.AddScoped<BayShiftService>();
hostBuilder.Services.AddScoped<BayShiftRepository>();

hostBuilder.Services.AddScoped<LoadService>();
hostBuilder.Services.AddScoped<LoadRepository>();

hostBuilder.Services.AddScoped<OperatingHourService>();
hostBuilder.Services.AddScoped<OperatingHourRepository>();

hostBuilder.Services.AddScoped<WorkService>();
hostBuilder.Services.AddScoped<WorkRepository>();

hostBuilder.Services.AddScoped<LocationService>();

hostBuilder.Services.AddScoped<ModelInitialize>();
hostBuilder.Services.AddScoped<ModelStepper>();
hostBuilder.Services.AddScoped<ModelState>();
hostBuilder.Services.AddScoped<ModelLogger>();
hostBuilder.Services.AddHostedService<ModelService>();


var host = hostBuilder.Build();
await host.RunAsync();
