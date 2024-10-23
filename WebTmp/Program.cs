using Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Abstractions;
using Services;
using WebTmp;

var hostBuilder = new HostApplicationBuilder(args);
hostBuilder.Services.AddDbContext<ModelDbContext>();

hostBuilder.Services.AddScoped<IAgentService, AdminStaffService>();
hostBuilder.Services.AddScoped<IAgentService, BayService>();
hostBuilder.Services.AddScoped<IAgentService, BayStaffService>();
hostBuilder.Services.AddScoped<IAgentService, CustomerService>();
hostBuilder.Services.AddScoped<IAgentService, HubService>();
hostBuilder.Services.AddScoped<IAgentService, TruckCompanyService>();
hostBuilder.Services.AddScoped<IAgentService, TruckDriverService>();
hostBuilder.Services.AddScoped<IAgentService, VendorService>();

hostBuilder.Services.AddHostedService<ModelStepper>();

var host = hostBuilder.Build();
await host.RunAsync();
