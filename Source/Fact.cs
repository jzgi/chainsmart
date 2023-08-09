using ChainFx;

namespace ChainSmart;

/// <summary>
/// The data modal for an event..
/// </summary>
public class Fact : Entity, IKeyable<short>
{
    public static readonly Fact Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "通知" },
        { 2, "其他" },
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
    internal short num;


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
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(num), ref num);
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
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(num), num);
        }
    }

    public short Key => typ;

    public override string ToString() => name;

    public short Index => num;
}