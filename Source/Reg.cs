using ChainFx;

namespace ChainSmart;

/// <summary>
/// A geographic or spatial region.
/// </summary>
public class Reg : Entity, IKeyable<short>, IFolderable
{
    public static readonly Reg Empty = new();

    public const short
        TYP_PROVINCE = 1,
        TYP_CITY = 2,
        TYP_SECTION = 3;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_PROVINCE, "省份" },
        { TYP_CITY, "地市" },
        { TYP_SECTION, "场区" },
    };

    internal short id;

    internal short idx;

    internal short num; // number of sub-resources

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        s.Get(nameof(idx), ref idx);
        s.Get(nameof(num), ref num);
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }

        s.Put(nameof(idx), idx);
        s.Put(nameof(num), num);
    }

    public short Key => id;

    public short Index => idx;

    public short Size => num;

    public bool IsProvince => typ == TYP_PROVINCE;

    public bool IsCity => typ == TYP_CITY;

    public bool IsSection => typ == TYP_SECTION;

    public override string ToString() => name;
}