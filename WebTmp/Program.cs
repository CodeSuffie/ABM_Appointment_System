using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Abstractions;
using Services;
using WebTmp;

var hostBuilder = new HostApplicationBuilder(args);
hostBuilder.Services.AddDbContext<ModelDbContext>();


hostBuilder.Services.AddScoped<IInitializationService, HubService>();
hostBuilder.Services.AddScoped<IInitializationService, TruckCompanyService>();
hostBuilder.Services.AddScoped<IInitializationService, AdminStaffService>();
hostBuilder.Services.AddScoped<IInitializationService, BayService>();
hostBuilder.Services.AddScoped<IInitializationService, BayStaffService>();
hostBuilder.Services.AddScoped<IInitializationService, ParkingSpotService>();
hostBuilder.Services.AddScoped<IInitializationService, TruckService>();


hostBuilder.Services.AddScoped<IStepperService, HubService>();
hostBuilder.Services.AddScoped<IStepperService, TruckCompanyService>();
hostBuilder.Services.AddScoped<IStepperService, AdminStaffService>();
hostBuilder.Services.AddScoped<IStepperService, BayService>();
hostBuilder.Services.AddScoped<IStepperService, BayStaffService>();
hostBuilder.Services.AddScoped<IStepperService, TripService>();
hostBuilder.Services.AddScoped<IStepperService, TruckService>();


hostBuilder.Services.AddScoped<AdminShiftService>();
hostBuilder.Services.AddScoped<BayShiftService>();
hostBuilder.Services.AddScoped<DropOffLoadService>();
hostBuilder.Services.AddScoped<LocationService>();
hostBuilder.Services.AddScoped<OperatingHourService>();
hostBuilder.Services.AddScoped<PickUpLoadService>();


hostBuilder.Services.AddHostedService<ModelStepper>();


var host = hostBuilder.Build();
await host.RunAsync();
