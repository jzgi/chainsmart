using ChainFX;

namespace ChainSmart;

/// <summary>
/// A batch move of goods.
/// </summary>
public class Bat : Entity, IKeyable<int>
{
    public static readonly Bat Empty = new();

    public const short
        TYP_ADD = 1,
        TYP_SRC = 2,
        TYP_PUR = 3,
        TYP_SUB = 4;


    public static readonly Map<short, string> Typs = new()
    {
        { TYP_ADD, "＋" },
        { TYP_SRC, "产源到货 ＋" },
        { TYP_PUR, "云仓到货 ＋" },
        { TYP_SUB, "－" },
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

    internal int stock;

    internal short tag;

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
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(tag), ref tag);
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(srcid), ref srcid);
        }
        else if ((msk & MSK_REV) == MSK_REV)
        {
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(tag), ref tag);
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(hubid), ref hubid);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(stock), ref stock);
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
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(qty), qty);
            s.Put(nameof(tag), tag);
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
            if (srcid > 0) s.Put(nameof(srcid), srcid); else s.PutNull(nameof(srcid));
        }
        else if ((msk & MSK_REV) == MSK_REV)
        {
            s.Put(nameof(qty), qty);
            s.Put(nameof(tag), tag);
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
            if (srcid > 0) s.Put(nameof(srcid), srcid); else s.PutNull(nameof(srcid));
            if (hubid > 0) s.Put(nameof(hubid), hubid); else s.PutNull(nameof(hubid));
        }
        
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(stock), stock);
        }

    }

    public int Key => id;

    public override string ToString() => name;
}