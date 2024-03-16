using ChainFX;

namespace ChainSmart;

/// <summary>
/// A standard definitive item..
/// </summary>
public class Std : Entity, IKeyable<short>, IFolderable
{
    public static readonly Std Empty = new();

    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };

    internal short idx;

    internal short style;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        s.Get(nameof(idx), ref idx);
        s.Get(nameof(style), ref style);
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        s.Put(nameof(idx), idx);
        s.Put(nameof(style), style);
    }

    public short Key => typ;

    public short Idx => idx;

    public short Style => style;

    public override string ToString() => name;
}

/// <summary>
/// A standard product category.
/// </summary>
public class Cat : Std
{
}

/// <summary>
/// A standard production environment.
/// </summary>
public class Env : Std
{
}

/// <summary>
/// A standard tracing tag.
/// </summary>
public class Tag : Std
{
}

/// <summary>
/// A standard product modification symbol.
/// </summary>
public class Sym : Std
{
}