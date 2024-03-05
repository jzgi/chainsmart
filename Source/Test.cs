using ChainFX;

namespace ChainSmart;

public class Test : Entity, IKeyable<int>
{
    public static readonly Test Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 10, "一般成分" },
        { 20, "感官" },
        { 30, "农药残留" },
        { 40, "饲料及添加剂" },
        { 41, "瘦肉精" },
        { 50, "兽药渔药残留" },
        { 51, "抗生素" },
        { 60, "重金属污染物" },
        { 70, "微生物" },
        { 80, "土壤养分" },
    };

    public static readonly Map<short, string> Levels = new()
    {
        { 1, "极差" },
        { 2, "异常" },
        { 3, "正常" },
        { 4, "优良" },
        { 5, "最佳" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤销" },
        { STU_CREATED, "创建" },
        { STU_ADAPTED, "调整" },
        { STU_OKED, "生效" },
    };

    internal int id;
    internal int estid;
    internal int orgid;
    internal decimal val;
    internal short level;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }
        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(estid), ref estid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(val), ref val);
            s.Get(nameof(level), ref level);
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
            s.Put(nameof(estid), estid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            if (orgid > 0) s.Put(nameof(orgid), orgid);
            else s.PutNull(nameof(orgid));

            s.Put(nameof(val), val);
            s.Put(nameof(level), level);
        }
    }

    public int Key => id;
}