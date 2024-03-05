using ChainFX;

namespace ChainSmart;

public class Aid : Entity, IKeyable<int>
{
    public static readonly Aid Empty = new();

    public const short
        TYP_ITEM = 1,
        TYP_LOT = 2,
        TYP_CODE = 5;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_ITEM, "商品" },
        { TYP_LOT, "产品" },
        { TYP_CODE, "溯源码" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤销" },
        { STU_CREATED, "创建" },
        { STU_ADAPTED, "调整" },
        { STU_OKED, "生效" },
    };

    internal int id;
    internal int orgid;
    internal int from;
    internal int dataid;

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
            s.Get(nameof(from), ref from);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(dataid), ref dataid);
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
            if (orgid > 0) s.Put(nameof(orgid), orgid);
            else s.PutNull(nameof(orgid));

            s.Put(nameof(from), from);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(dataid), dataid);
        }
    }

    public int Key => id;
}