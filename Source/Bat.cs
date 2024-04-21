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
        { TYP_INC, "加数 ＋" },
        { TYP_SRC, "产源 ＋" },
        { TYP_PUR, "采购 ＋" },
        { TYP_DEC, "直减 －" },
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
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(srcid), ref srcid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(hubid), ref hubid);
            s.Get(nameof(qty), ref qty);
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
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(srcid), srcid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            if (hubid > 0) s.Put(nameof(hubid), hubid);
            else s.PutNull(nameof(hubid));
            s.Put(nameof(qty), qty);
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