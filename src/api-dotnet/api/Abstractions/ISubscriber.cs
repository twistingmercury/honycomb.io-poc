namespace TM.PoC.API.Abstractions;

public interface ISubscriber<T> where T : class, new()
{
    Task Subscribe(T t);
}