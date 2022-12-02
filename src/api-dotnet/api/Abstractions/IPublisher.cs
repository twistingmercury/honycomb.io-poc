namespace TM.PoC.API.Abstractions;

public interface IPublisher<in T> where T : class, new()
{
    Task Publish(T t);
}