using System.Text;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TM.PoC.API.Messaging.RabbitMQ;

public class RabbitMQSubscriber<T> : BackgroundService where T : class, new()
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly string _fullName;
    private readonly BasicSubscriberHandler<T> _handler;
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private readonly Tracer _tracer;

    public RabbitMQSubscriber(IConnectionFactory factory,
        BasicSubscriberHandler<T> handler, Tracer tracer)
    {
        _tracer = tracer;
        _handler = handler;
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            "new_widgets",
            false,
            false,
            false,
            null);
        _fullName = typeof(RabbitMQSubscriber<T>).FullName!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnConsumerOnReceived;
        _channel.BasicConsume("new_widgets", false, consumer);
        await Task.CompletedTask;
    }

    private async Task OnConsumerOnReceived(object sender, BasicDeliverEventArgs args)
    {
        var parentContext = _propagator.Extract(default, args.BasicProperties, ExtractTraceContext);
        Baggage.Current = parentContext.Baggage;
        var spanName = $"{_fullName}.{nameof(OnConsumerOnReceived)}";

        using var span =
            _tracer.StartActiveSpan(spanName, SpanKind.Consumer, new SpanContext(parentContext.ActivityContext));

        if (span is null) throw new ApplicationException("subscriber span is null");

        try
        {
            await _handler.Handle(args.Body.ToArray());
            _channel.BasicAck(args.DeliveryTag, false);
            span.SetStatus(Status.Ok);
        }
        catch (Exception ex)
        {
            var bex = ex.GetBaseException();
            span.SetStatus(Status.Error);
            span.SetAttribute("error_type", bex.GetType().Name);
            span.SetAttribute("error_msg", bex.Message);
            span.SetAttribute("stack_trace", bex.StackTrace ?? "[not available]");
            _channel.BasicNack(args.DeliveryTag, false, false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _channel.Close();
        _connection.Close();
    }

    private IEnumerable<string> ExtractTraceContext(IBasicProperties props, string key)
    {
        if (!props.Headers.TryGetValue(key, out var value)) return Enumerable.Empty<string>();
        var bytes = value as byte[];
        return new[] { Encoding.UTF8.GetString(bytes ?? Array.Empty<byte>()) };
    }
}