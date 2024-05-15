using ChainFX;

namespace ChainSmart;

/// <summary>
/// A range of tracebility codes. 
/// </summary>
public class Code : Entity, IKeyable<int>
{
    public static readonly Code Empty = new();

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "作废" },
        { STU_CREATED, "创建" },
        { STU_ADAPTED, "提交" },
        { STU_OKED, "发放" },
    };


    internal int id;
    internal int orgid;
    internal int num;
    internal string addr;
    internal int nstart;
    internal int nend;
    internal string ship;


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
            s.Get(nameof(num), ref num);
            s.Get(nameof(addr), ref addr);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(ship), ref ship);
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
            s.Put(nameof(num), num);
            s.Put(nameof(addr), addr);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
            s.Put(nameof(ship), ship);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}