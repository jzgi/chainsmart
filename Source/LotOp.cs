using ChainFX;

namespace ChainSmart;

/// <summary>
/// A lot operation to particular hub.
/// </summary>
public class LotOp : Entity, IKeyable<int>
{
    public static readonly LotOp Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "进仓 ＋" },
        { 2, "出仓 －" },
        { 3, "盘盈 ＋" },
        { 4, "盘亏 －" },
        { 5, "增益 ＋" },
        { 6, "损耗 －" },
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

    internal int lotid;

    internal int hubid;

    internal int qty;

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
            s.Get(nameof(lotid), ref lotid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(hubid), ref hubid);
            s.Get(nameof(qty), ref qty);
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
            s.Put(nameof(lotid), lotid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(hubid), hubid);
            s.Put(nameof(qty), qty);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}