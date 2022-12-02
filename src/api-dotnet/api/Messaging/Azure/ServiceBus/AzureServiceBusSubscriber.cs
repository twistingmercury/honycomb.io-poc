using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using OpenTelemetry.Trace;
using TM.PoC.API.Abstractions;

namespace TM.PoC.API.Messaging.Azure.ServiceBus;

public class AzureServiceBusSubscriber<T> : BackgroundService where T : class, new()
{
    private readonly IAsyncMessageHandler<T> _handler;
    private readonly ServiceBusProcessor _processor;
    private readonly Tracer _tracer;

    public AzureServiceBusSubscriber(ServiceBusProcessor processor, IAsyncMessageHandler<T> handler, Tracer tracer)
    {
        _handler = handler;
        _processor = processor;
        _tracer = tracer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += _ => Task.CompletedTask; // don't care - just a PoC
        await _processor.StartProcessingAsync(stoppingToken);
    }

    protected virtual async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        using var span = StartTelemetrySpan(args,
            $"{GetType().Namespace}.{nameof(AzureServiceBusSubscriber<T>)}.{nameof(HandleMessageAsync)}");
        await _handler.HandleAsync(args);
        await args.CompleteMessageAsync(args.Message);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private TelemetrySpan StartTelemetrySpan(ProcessMessageEventArgs args, string spanName)
    {
        var traceparent = args.Message.ApplicationProperties["traceparent"] as string;
        var spanparent = args.Message.ApplicationProperties["spanparent"] as string;

        if (string.IsNullOrWhiteSpace(traceparent) || string.IsNullOrWhiteSpace(spanparent))
            return _tracer.StartActiveSpan(spanName, SpanKind.Consumer);

        var actTId = ActivityTraceId.CreateFromBytes(Convert.FromHexString(traceparent));
        var actSId = ActivitySpanId.CreateFromBytes(Convert.FromHexString(spanparent));
        var ctx = new SpanContext(actTId, actSId, ActivityTraceFlags.Recorded, true, null);

        return _tracer.StartActiveSpan(spanName, SpanKind.Consumer, ctx);
    }
}