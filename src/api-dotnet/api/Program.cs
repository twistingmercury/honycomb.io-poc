using System.Configuration;
using OpenTelemetry.Resources;
using TM.Decorators.Logging;
using TM.Decorators.Tracing;
using TM.PoC.API.Abstractions;
using TM.PoC.API.Startup;
using TM.PoC.API.Widgets.DataAccess;
using TM.PoC.API.Widgets.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var serviceName = builder.Configuration["SERVICE_NAME"] ??
                  throw new ConfigurationErrorsException("missing value for `SERVICE_NAME`");
var serviceVersion = builder.Configuration["SERVICE_VERSION"] ??
                     throw new ConfigurationErrorsException("missing value for `SERVICE_NAME`");

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion);

builder.Services.AddOtelTracing(builder.Configuration, resourceBuilder);
builder.Services.AddOtelLogging(builder.Configuration, resourceBuilder);
builder.Services.AddWidgetRepository();
builder.Services.AddAzureServiceBus();
builder.Services.AddEndpoints(typeof(WidgetEndpoints));

var app = builder.Build();
app.UseEndpoints();
await app.RunAsync();