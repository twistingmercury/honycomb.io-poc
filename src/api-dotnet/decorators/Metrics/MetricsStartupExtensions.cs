// using System.Diagnostics.Metrics;
// using Microsoft.Extensions.DependencyInjection;
// using OpenTelemetry;
// using OpenTelemetry.Exporter;
// using OpenTelemetry.Metrics;
//
// namespace Decorators.Extensions;
//
// public static class MetricsStartupExtensions
// {
//     public static IServiceCollection AddMetrics(this IServiceCollection services, string serviceName,
//         string serviceVersion)
//     {
//         Meter appMeter = new(serviceName, serviceVersion);
//         var totalCallsCounter = appMeter.CreateCounter<long>("total_calls");
//         var errCounter = appMeter.CreateCounter<long>("total_errs_calls");
//         var sucCounter = appMeter.CreateCounter<long>("total_success_calls");
//         
//
//         var meterProvider = Sdk.CreateMeterProviderBuilder()
//             .AddMeter(serviceName)
//             .AddOtlpExporter(opt =>
//             {
//                 var ep = Environment.GetEnvironmentVariable("OTEL_EP") ??
//                          throw new ApplicationException("Environment var 'OTEL_EP' is missing.");
//                 opt.Endpoint = new Uri(ep);
//                 opt.Protocol = OtlpExportProtocol.Grpc;
//             })
//             .Build();
//
//         return services.AddSingleton(meterProvider);
//     }
// }

