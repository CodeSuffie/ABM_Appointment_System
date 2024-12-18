using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Simulator.Extensions;

var hostBuilder = new HostApplicationBuilder(args);
hostBuilder.Services.AddSimulator();

var host = hostBuilder.Build();

var modelService = host.Services.GetRequiredService<ModelService>();

await modelService.InitializeAsync();
await host.StartAsync();
await modelService.RunAsync();
