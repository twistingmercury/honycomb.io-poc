namespace TM.Decorators.Tracing;

public class TraceException : Exception
{
    public TraceException()
    {
    }

    public TraceException(string? message) : base(message)
    {
    }

    public TraceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}