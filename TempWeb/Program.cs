using TempWeb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ModelDbContext>();

var app = builder.Build();

app.Run();
