using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using TM.PoC.API.Abstractions;

namespace TM.PoC.API.Messaging.RabbitMQ;

public class RabbitMQPublisher<T> : IPublisher<T> where T : class, new()
{
    private readonly IConnectionFactory _factory;
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private readonly Tracer _tracer;

    public RabbitMQPublisher(IConnectionFactory factory, Tracer tracer)
    {
        _factory = factory;
        _tracer = tracer;
    }

    public Task Publish(T t)
    {
        using var span = _tracer.StartActiveSpan($"{GetType().FullName}.{nameof(Publish)}");
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            "new_widgets",
            false,
            false,
            false,
            null);

        var props = channel.CreateBasicProperties();
        var propCtx = new PropagationContext(span.Context, Baggage.Current);
        _propagator.Inject(propCtx, props, InjectTraceContext);

        var payload = ToBytes(t);

        channel.BasicPublish(
            "widgets_exchange",
            "",
            props,
            payload);

        return Task.CompletedTask;
    }

    private void InjectTraceContext(IBasicProperties props, string key, string value)
    {
        props.Headers ??= new Dictionary<string, object>();
        props.Headers[key] = value;
    }

    private static byte[] ToBytes(T t)
    {
        var json = JsonSerializer.Serialize(t);
        return Encoding.UTF8.GetBytes(json);
    }
}