namespace TM.PoC.API.Abstractions;

public interface ISubscriberHandler
{
    Task Handle(byte[] data);
}