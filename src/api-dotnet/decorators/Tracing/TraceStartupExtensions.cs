using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TM.Decorators.Tracing;

/// <summary>
///     TraceStartupExtensions abstracts the bootstrapping of OpenTelemetry tracing.
/// </summary>
public static class TraceStartupExtensions
{
    public static void AddOtelTracing(this IServiceCollection services, IConfiguration cfg, ResourceBuilder rb)
    {
        var otelUri = cfg["OTEL_URI"] ??
                      throw new ConfigurationErrorsException("missing value for `OTEL_URI`");
        var svcName = cfg["SERVICE_NAME"] ??
                      throw new ConfigurationErrorsException("missing value for `SERVICE_NAME`");

        services.AddOpenTelemetryTracing(builder => builder
            .AddSource(svcName)
            .SetResourceBuilder(rb)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(option =>
            {
                option.Endpoint = new Uri(otelUri);
                option.Protocol = OtlpExportProtocol.Grpc;
            })
        );

        var defaultTracer = TracerProvider.Default.GetTracer(svcName);

        services.AddSingleton(defaultTracer);

        TraceDecoratorFactory.SetDefaultTracer(defaultTracer);
    }
}