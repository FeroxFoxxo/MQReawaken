namespace Server.Base.Core.Abstractions;

public abstract class Module
{
    public abstract int Major { get; }

    public abstract int Minor { get; }

    public abstract int Patch { get; }

    public virtual string GetModuleInformation() =>
        GetType().Assembly.FullName?.Split('.')[^1] +
        $" v{Major}.{Minor}.{Patch}";
}
