namespace MCG.PlatformServices.Decorators;

public enum ProtectionStrategy
{
    Redact,
    Hash,
    Omit
}
[AttributeUsage(AttributeTargets.Parameter)]
public class ProtectAttribute : System.Attribute
{
    public ProtectAttribute():this(ProtectionStrategy.Redact)
    {
    }
    
    public ProtectAttribute( ProtectionStrategy strategy)
    {
        this.Strategy = strategy;
    }

    public ProtectionStrategy Strategy { get; }
}