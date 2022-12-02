using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Trace;
using TM.PoC.API.Abstractions;

namespace TM.PoC.API.Messaging.Azure.ServiceBus;

public class AzureServiceBusPublisher<T> : IPublisher<T> where T : class, new()
{
    private readonly ServiceBusClient _client;
    private readonly string _entityPath;
    private readonly Tracer _tracer;

    public AzureServiceBusPublisher(ServiceBusClient client, string entityPath, Tracer tracer)
    {
        _client = client;
        _entityPath = entityPath;
        _tracer = tracer;
    }

    Task IPublisher<T>.Publish(T t)
    {
        return PublishAsync(t);
    }

    public async Task PublishAsync(T t)
    {
        using var span = _tracer.StartActiveSpan($"{GetType().FullName}.{nameof(PublishAsync)}");
        Debug.Assert(span != null);

        var payload = ToBytes(t);
        ServiceBusMessage msg = new(new BinaryData(payload)) { ContentType = "application/json" };
        msg.ApplicationProperties.Add("traceparent", span.Context.TraceId.ToHexString());
        msg.ApplicationProperties.Add("spanparent", span.Context.SpanId.ToHexString());
        var sender = _client.CreateSender(_entityPath);
        await sender.SendMessageAsync(msg);
    }

    private static byte[] ToBytes(T t)
    {
        var json = JsonSerializer.Serialize(t);
        return Encoding.UTF8.GetBytes(json);
    }
}