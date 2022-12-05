using OpenTelemetry.Trace;

namespace TM.Decorators.Tracing;

public static class SpanExtensions
{
    public static void AddEventInfo(this TelemetrySpan span, string name, params SpanAttribute[]? data)
    {
        var attributes = data == null
            ? new SpanAttributes()
            : new SpanAttributes(SpanAttribute.ToIEnumerable(data));

        span.AddEvent(
            name: name
                .Replace(' ', '_')
                .Replace("__", "_"),
            timestamp: DateTimeOffset.Now,
            attributes: attributes);
    }
}

public class SpanAttribute
{
    public SpanAttribute(string key, params object[] values)
    {
        Key = key
            .Replace(' ', '_')
            .Replace("__", "_");

        Values = values;
    }

    public string Key { get; }
    public object Values { get; }

    private KeyValuePair<string, object> ToKVP() => new(Key, Values);

    public static IEnumerable<KeyValuePair<string, object>> ToIEnumerable(params SpanAttribute[] attributes)
    {
        return attributes.Select(at => at.ToKVP()).ToList();
    }
}