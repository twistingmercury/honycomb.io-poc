using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace TM.Decorators.Logging;

/// <summary>
///     LoggingStartupExtensions abstracts bootstrapping of OpenTelemetry logging.
/// </summary>
public static class LoggingStartupExtensions
{
    public static void AddOtelLogging(this IServiceCollection services, IConfiguration cfg, ResourceBuilder rb)
    {
        var otelUri = cfg["OTEL_URI"] ??
                      throw new ConfigurationErrorsException("missing value for `OTEL_URI`");

        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(rb);
                options.AddOtlpExporter(option =>
                {
                    option.Endpoint = new Uri(otelUri);
                    option.Protocol = OtlpExportProtocol.Grpc;
                });
            });
        });
    }
}