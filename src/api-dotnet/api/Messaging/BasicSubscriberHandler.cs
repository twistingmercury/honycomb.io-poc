using System.Text;
using System.Text.Json;
using TM.Decorators.Tracing;
using TM.PoC.API.Abstractions;

namespace TM.PoC.API.Messaging;

public abstract class BasicSubscriberHandler<T> : ISubscriberHandler where T : new()
{
    [Trace]
    public abstract Task Handle(byte[] data);

    protected virtual T FromBytes(byte[] bytes)
    {
        if (bytes == null || !bytes.Any()) throw new ArgumentNullException(nameof(bytes));
        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}