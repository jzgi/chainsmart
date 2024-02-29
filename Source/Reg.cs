using ChainFX;

namespace ChainSmart;

/// 
/// A geographic or functional division.
/// 
public class Reg : Entity, IKeyable<short>, IFolderable
{
    public static readonly Reg Empty = new();

    public const short
        TYP_SECTOR = 1,
        TYP_CITY = 2,
        TYP_PROVINCE = 3;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_SECTOR, "版块" },
        { TYP_CITY, "地市" },
        { TYP_PROVINCE, "省份" },
    };

    internal short id;

    internal short idx;

    internal short style;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        s.Get(nameof(idx), ref idx);
        s.Get(nameof(style), ref style);
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }

        s.Put(nameof(idx), idx);
        s.Put(nameof(style), style);
    }

    public short Key => id;

    public short Index => idx;

    public short Size => style;

    public bool IsProvince => typ == TYP_PROVINCE;

    public bool IsCity => typ == TYP_CITY;

    public bool IsSector => typ == TYP_SECTOR;

    public override string ToString() => name;
}