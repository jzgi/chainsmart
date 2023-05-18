using ChainFx;

namespace ChainSmart;

public class Eval : Entity, IKeyable<int>
{
    public static readonly Cat Empty = new();

    internal int id;
    internal int orgid;
    internal JArr data;

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
            s.Get(nameof(data), ref data);
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
            s.Put(nameof(data), data);
        }
    }

    public int Key => id;
}