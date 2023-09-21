using ChainFx;

namespace ChainSmart;

/// <summary>
/// An job incubation program. 
/// </summary>
public class Job : Entity, IKeyable<int>
{
    public static readonly Job Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "创业卡" },
        { 2, "助农卡" },
        { 4, "振兴卡" },
    };

    public static readonly Map<short, string> Tips = new()
    {
        { 1, "发放对象：优质高标绿码农业生产者" },
        { 2, "发放对象：退役军人、大学生、农村青年，赋能孵化对象" },
        { 4, "发放对象：流动商贩安置、零星种植兼职售卖、困难人群、不便人群及老人等。" },
    };

    internal int id;
    internal int upperid;
    internal int userid;
    internal string idno;
    internal string cardno;
    internal decimal bal;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(upperid), ref upperid);
            s.Get(nameof(userid), ref userid);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(idno), ref idno);
            s.Get(nameof(cardno), ref cardno);
            s.Get(nameof(bal), ref bal);
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
            s.Put(nameof(upperid), upperid);
            s.Put(nameof(userid), userid);
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(idno), idno);
            s.Put(nameof(cardno), cardno);
            s.Put(nameof(bal), bal);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}