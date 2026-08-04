using EcommerceAPI.Application;
using EcommerceAPI.Extensions;
using EcommerceAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

var app = builder.Build();

app.UseAppPipeline();

app.Run();