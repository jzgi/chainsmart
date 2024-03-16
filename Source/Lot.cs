using ChainFX;

namespace ChainSmart;

/// <summary>
/// A lot operation flow to particular hub.
/// </summary>
public class Lot : Entity, IKeyable<int>
{
    public static readonly Lot Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "进仓 ＋" },
        { 2, "出仓 －" },
        { 3, "盘盈 ＋" },
        { 4, "盘亏 －" },
        { 5, "增益 ＋" },
        { 6, "损耗 －" },
        { 7, "冲加 ＋" },
        { 8, "冲减 －" },
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

    internal int hubid;

    internal int stock;

    internal short area;

    // must have an icon

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
            s.Get(nameof(hubid), ref hubid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(area), ref area);
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
            s.Put(nameof(hubid), hubid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(area), area);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(stock), stock);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}