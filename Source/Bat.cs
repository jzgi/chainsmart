using ChainFX;

namespace ChainSmart;

/// <summary>
/// A batch move of goods.
/// </summary>
public class Bat : Entity, IKeyable<int>
{
    public static readonly Bat Empty = new();

    public const short
        TYP_INC = 1,
        TYP_SRC = 2,
        TYP_PUR = 3,
        TYP_DEC = 4,
        TYP_WST = 5,
        TYP_LOS = 6;


    public static readonly Map<short, string> Typs = new()
    {
        { TYP_INC, "改数 ＋" },
        { TYP_SRC, "从产源调货 ＋" },
        { TYP_PUR, "云仓到货 ＋" },
        { TYP_DEC, "改数 －" },
        { TYP_WST, "损耗 －" },
        { TYP_LOS, "盘亏 －" },
    };


    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "作废" },
        { STU_CREATED, "新建" },
        { STU_ADAPTED, "调整" },
        { STU_OKED, "发布" },
    };


    internal int id;

    internal int orgid;

    internal int itemid;

    internal int srcid;

    internal int hubid;

    internal int qty;

    internal string unit;

    internal short tag; // tag type

    internal int nstart;

    internal int nend;


    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }
        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(orgid), ref orgid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(hubid), ref hubid);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(unit), ref unit);
        }
        if ((msk & MSK_LATE) == MSK_LATE)
        {
            s.Get(nameof(tag), ref tag);
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
        }
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }
        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Put(nameof(orgid), orgid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(itemid), itemid);
            if (srcid > 0) s.Put(nameof(srcid), srcid);
            else s.PutNull(nameof(srcid));
            if (hubid > 0) s.Put(nameof(hubid), hubid);
            else s.PutNull(nameof(hubid));
            s.Put(nameof(qty), qty);
            s.Put(nameof(unit), unit);
        }
        if ((msk & MSK_LATE) == MSK_LATE)
        {
            s.Put(nameof(tag), tag);
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}