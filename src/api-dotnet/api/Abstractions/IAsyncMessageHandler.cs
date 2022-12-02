using Azure.Messaging.ServiceBus;

namespace TM.PoC.API.Abstractions;

public interface IAsyncMessageHandler<T> where T : class, new()
{
    Task HandleAsync(ProcessMessageEventArgs args);
}