using ChainFX;

namespace ChainSmart;

/// <summary>
/// A standard definitive item..
/// </summary>
public class Def : Entity, IKeyable<short>
{
    public static readonly Def Empty = new();

    public const short
        SUB_CAT = 1, SUB_TAG = 2, SUB_SYM = 3, SUB_CER = 4;

    public static readonly Map<short, DefDescr> Descrs = new()
    {
        new DefDescr(
            SUB_CAT, "品类", "cats"
        ),
        new DefDescr(
            SUB_TAG, "溯源", "tags"
        ),
        new DefDescr(
            SUB_SYM, "标志", "syms"
        ),
        new DefDescr(
            SUB_CER, "认证", "cers"
        ),
    };

    internal short idx;

    internal short style;

    public static readonly Map<short, string> Styles = new()
    {
        { 0, "默认" }
    };

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

public class DefDescr : IKeyable<short>
{
    readonly short sub;

    readonly string title;

    readonly string dbtable;

    public DefDescr(short sub, string title, string dbtable)
    {
        this.sub = sub;
        this.title = title;
        this.dbtable = dbtable;
    }

    public short Key => sub;

    public short Sub => sub;

    public string Title => title;

    public string DbTable => dbtable;
}

/// <summary>
/// A standard product category.
/// </summary>
public class Cat : Def
{
}

/// <summary>
/// A standard tracing tag.
/// </summary>
public class Tag : Def
{
}

/// <summary>
/// A standard product modification symbol.
/// </summary>
public class Sym : Def
{
}

/// <summary>
/// A standard certification program that a user can attend..
/// </summary>
public class Cer : Def
{
}