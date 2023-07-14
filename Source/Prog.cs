using ChainFx;

namespace ChainSmart;

/// <summary>
/// A incubation program. 
/// </summary>
public class Prog : Entity, IKeyable<(int, short)>
{
    public static readonly Prog Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "城乡振兴卡" },
        { 2, "民生创业卡" },
        { 4, "民生助农卡" },
    };

    public static readonly Map<short, string> Tips = new()
    {
        { 1, "发放对象：优质高标绿码农业生产者" },
        { 2, "发放对象：退役军人、大学生、农村青年，赋能孵化对象" },
        { 4, "发放对象：流动商贩安置、零星种植兼职售卖、困难人群、不便人群及老人等。" },
    };

    internal int userid;
    internal decimal bal;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(userid), ref userid);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(bal), ref bal);
        }
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        base.Write(s, msk);

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Put(nameof(userid), userid);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(bal), bal);
        }
    }

    public (int, short) Key => (userid, typ);

    public override string ToString() => name;
}