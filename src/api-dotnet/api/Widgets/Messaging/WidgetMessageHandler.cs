using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TM.Decorators.Tracing;
using TM.PoC.API.Abstractions;
using TM.PoC.API.Widgets.Types;

namespace TM.PoC.API.Widgets.Messaging;

public class WidgetMessageHandler : IAsyncMessageHandler<Widget>
{
    private readonly IDataRepository<Widget> _repo;

    public WidgetMessageHandler(IDataRepository<Widget> repo)
    {
        _repo = repo;
    }

    [Trace]
    public async Task HandleAsync(ProcessMessageEventArgs args)
    {
        var newWidget = FromBytes(args.Message);
        await _repo.CreateAsync(newWidget);
    }

    [Trace]
    public Widget FromBytes(ServiceBusReceivedMessage msg)
    {
        if (msg?.Body == null) throw new ArgumentNullException(nameof(msg));
        var bytes = msg.Body.ToArray();
        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<Widget>(json)!;
    }
}