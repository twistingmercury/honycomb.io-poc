using Azure.Messaging.ServiceBus;
using OpenTelemetry.Trace;
using TM.PoC.API.Abstractions;
using TM.PoC.API.Messaging.Azure.ServiceBus;
using TM.PoC.API.Widgets.Messaging;
using TM.PoC.API.Widgets.Types;

namespace TM.PoC.API.Startup;

public static class AzureServiceBusStartupExtensions
{
    public static void AddAzureServiceBus(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("WIDGET_QUEUE_CONNECTION") ??
                               throw new ApplicationException("missing 'WIDGET_QUEUE_CONNECTION' env var");

        var entityPath = Environment.GetEnvironmentVariable("WIDGET_ENTITY_PATH") ??
                         throw new ApplicationException("missing 'WIDGET_ENTITY_PATH' env var");

        var sbClient = new ServiceBusClient(connectionString, new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        });

        services.AddSingleton(_ => sbClient);
        services.AddTransient<IPublisher<Widget>>(p =>
        {
            var tracer = p.GetRequiredService<Tracer>();
            return new AzureServiceBusPublisher<Widget>(sbClient, entityPath, tracer);
        });
        services.AddSingleton<IAsyncMessageHandler<Widget>>(p =>
        {
            var repo = p.GetRequiredService<IDataRepository<Widget>>();
            return new WidgetMessageHandler(repo);
        });
        services.AddHostedService(p =>
        {
            var tracer = p.GetRequiredService<Tracer>();
            var client = p.GetRequiredService<ServiceBusClient>();
            return new AzureServiceBusSubscriber<Widget>(
                client.CreateProcessor(entityPath),
                p.GetRequiredService<IAsyncMessageHandler<Widget>>(),
                tracer);
        });
    }
}