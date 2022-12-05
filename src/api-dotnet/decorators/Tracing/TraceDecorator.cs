using System.Diagnostics;
using System.Reflection;
using AspectInjector.Broker;
using OpenTelemetry.Trace;
using TM.Decorators.Abstractions;

namespace TM.Decorators.Tracing;

/// <summary>
///     TraceDecoratorFactory is used to create a new <see cref="TraceDecorator" />
/// </summary>
public static class TraceDecoratorFactory
{
    public static Tracer DefaultTracer { get; private set; } = default!;

    internal static void SetDefaultTracer(Tracer defaultTrader)
    {
        DefaultTracer = defaultTrader ?? throw new ArgumentNullException(nameof(defaultTrader));
    }

    public static object GetInstance(Type aspectType)
    {
        return new TraceDecorator(DefaultTracer);
    }
}

/// <summary>
///     TraceAttributed is what is used to decorate the target and to inject the <see cref="TraceDecorator" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[Injection(typeof(TraceDecorator))]
public class TraceAttribute : BaseUniversalWrapperAttribute
{
}

/// <summary>
///     TraceDecorator provides boilerplate distributed tracing when applied to a class, or methods.
/// </summary>
[Aspect(Scope.Global, Factory = typeof(TraceDecoratorFactory))]
public sealed class TraceDecorator : BaseUniversalWrapperAspect
{
    private readonly Tracer _tracer;

    public TraceDecorator(Tracer tracer)
    {
        _tracer = tracer;
    }

    [Advice(Kind.Around, Targets = Target.Method)]
    public object Handle(
        [Argument(Source.Instance)] object instance,
        [Argument(Source.Type)] Type type,
        [Argument(Source.Metadata)] MethodBase method,
        [Argument(Source.Target)] Func<object[], object> target,
        [Argument(Source.Name)] string name,
        [Argument(Source.Arguments)] object[] args,
        [Argument(Source.ReturnType)] Type returnType,
        [Argument(Source.Triggers)] Attribute[] triggers)
    {
        using var span = _tracer.StartActiveSpan($"{type.FullName}.{method.Name}");
        Debug.Assert(span is not null);

        try
        {
            var result = BaseHandle(instance, type, method, target, name, args, returnType, triggers);
            span.SetStatus(Status.Ok);
            return result;
        }
        catch (Exception ex)
        {
            var bex = ex.GetBaseException();
            span.SetStatus(Status.Error);
            span.SetAttribute("error_type", bex.GetType().Name);
            span.SetAttribute("error_msg", bex.Message);
            span.SetAttribute("stack_trace", bex.StackTrace ?? "[not available]");
            throw;
        }
    }
}

public static class TraceExtensions
{
    public static void AddEvent(this Tracer t, string name, IDictionary<string, string>? attributes = null)
    {
        SpanAttributes spanData = new();
        if (attributes is not null)
        {
            foreach (var attrib in attributes)
            {
                spanData.Add(attrib.Key, attrib.Value);
            }
        }

        Tracer.CurrentSpan.AddEvent(name: name, attributes: spanData);
    }
}