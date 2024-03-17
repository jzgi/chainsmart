using ChainFX;

namespace ChainSmart;

/// <summary>
/// A standard definitive item..
/// </summary>
public class Std : Entity, IKeyable<short>, IFolderable
{
    public static readonly Std Empty = new();

    public const int SUB_CAT = 1, SUB_ENV = 2, SUB_TAG = 3, SUB_SYM = 4, SUB_CER = 5;

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

    public static string DbTableOf(int sub) => sub switch
    {
        SUB_CAT => "cats", SUB_ENV => "envs", SUB_TAG => "tags", SUB_SYM => "syms", _ => "cers"
    };

    public static string TitleOf(int sub) => sub switch
    {
        SUB_CAT => "品类", SUB_ENV => "环境", SUB_TAG => "溯源", SUB_SYM => "标符", _ => "认证"
    };
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

/// <summary>
/// A standard certification program that a user can attend..
/// </summary>
public class Cer : Std
{
}