using ChainFX;

namespace ChainSmart;

/// <summary>
/// A standard definitive item..
/// </summary>
public class Std : Entity, IKeyable<short>, IFolderable
{
    public static readonly Std Empty = new();

    public const short
        SUB_CAT = 1, SUB_ENV = 2, SUB_TAG = 3, SUB_SYM = 4, SUB_CER = 5;

    public static readonly Map<short, StdDescr> Descrs = new()
    {
        new StdDescr(
            SUB_CAT, "品类", "cats", Cat.Styles
        ),
        new StdDescr(
            SUB_ENV, "环境", "envs", Env.Styles
        ),
        new StdDescr(
            SUB_TAG, "溯源", "tags", Tag.Styles
        ),
        new StdDescr(
            SUB_SYM, "标志", "syms", Sym.Styles
        ),
        new StdDescr(
            SUB_CER, "认证", "cers", Cer.Styles
        ),
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

public class StdDescr : IKeyable<short>
{
    readonly short sub;

    readonly string title;

    readonly string dbtable;

    readonly Map<short, string> styles;

    public StdDescr(short sub, string title, string dbtable, Map<short, string> styles)
    {
        this.sub = sub;
        this.title = title;
        this.dbtable = dbtable;
        this.styles = styles;
    }

    public short Key => sub;

    public short Sub => sub;

    public string Title => title;

    public string DbTable => dbtable;

    public Map<short, string> Styles => styles;
}

/// <summary>
/// A standard product category.
/// </summary>
public class Cat : Std
{
    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };
}

/// <summary>
/// A standard production environment.
/// </summary>
public class Env : Std
{
    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };
}

/// <summary>
/// A standard tracing tag.
/// </summary>
public class Tag : Std
{
    public static readonly Map<short, string> Styles = new()
    {
        { 1, "硬牌" },
        { 2, "软牌" },
        { 3, "贴标" },
        { 4, "芯片" },
    };

    public override string ToString() => name + Styles[style];
}

/// <summary>
/// A standard product modification symbol.
/// </summary>
public class Sym : Std
{
    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };
}

/// <summary>
/// A standard certification program that a user can attend..
/// </summary>
public class Cer : Std
{
    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };
}