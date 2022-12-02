// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Reflection;

namespace TM.Decorators.Abstractions;

public class AspectEventArgs : EventArgs
{
    public object Instance { get; internal init; } = null!;
    public Type Type { get; internal init; } = null!;
    public MethodBase Method { get; internal init; } = null!;
    public string Name { get; internal init; } = null!;
    public IReadOnlyList<object> Args { get; internal init; } = null!;
    public Type ReturnType { get; internal init; } = null!;
    public Attribute[] Triggers { get; internal init; } = null!;
}