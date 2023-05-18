using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// A supply product lot record.
/// </summary>
public class Lot : Entity, IKeyable<int>
{
    public static readonly Lot Empty = new();

    public const short
        TYP_SPOT = 1,
        TYP_PRE = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_SPOT, "现货" },
        { TYP_PRE, "助农" },
    };

    internal int id;
    internal int orgid;
    internal string orgname;

    internal int[] targs; // (optional) targeted centers or markets
    internal short catid;
    internal DateTime started;
    internal int fabid;
    internal string unit;
    internal short unitw;
    internal short unitx;
    internal decimal price;
    internal decimal off;
    internal int capx;
    internal int stock;
    internal int avail;
    internal short minx;
    internal short maxx;
    internal short flashx;

    // traceability
    internal int nstart;
    internal int nend;

    // media
    internal bool icon;
    internal bool pic;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool m4;

    internal StockOp[] ops;

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
            s.Get(nameof(orgname), ref orgname);
            s.Get(nameof(stock), ref stock);
            s.Get(nameof(avail), ref avail);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(fabid), ref fabid);
            s.Get(nameof(targs), ref targs);
            s.Get(nameof(catid), ref catid);
            s.Get(nameof(started), ref started);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitw), ref unitw);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(capx), ref capx);
            s.Get(nameof(minx), ref minx);
            s.Get(nameof(maxx), ref maxx);
            s.Get(nameof(flashx), ref flashx);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
            s.Get(nameof(m1), ref m1);
            s.Get(nameof(m2), ref m2);
            s.Get(nameof(m3), ref m3);
            s.Get(nameof(m4), ref m4);
        }

        if ((msk & MSK_EXTRA) == MSK_EXTRA)
        {
            s.Get(nameof(ops), ref ops);
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
            s.Put(nameof(orgname), orgname);
            s.Put(nameof(stock), stock);
            s.Put(nameof(avail), avail);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(fabid), fabid);
            s.Put(nameof(targs), targs);
            s.Put(nameof(catid), catid);
            s.Put(nameof(started), started);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitw), unitw);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(capx), capx);
            s.Put(nameof(minx), minx);
            s.Put(nameof(maxx), maxx);
            s.Put(nameof(flashx), flashx);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);

            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
            s.Put(nameof(m1), m1);
            s.Put(nameof(m2), m2);
            s.Put(nameof(m3), m3);
            s.Put(nameof(m4), m4);
        }

        if ((msk & MSK_EXTRA) == MSK_EXTRA)
        {
            s.Put(nameof(ops), ops);
        }
    }

    public int Key => id;

    // STATE
    //

    public const short STA_OKABLE = 1;

    public override short State
    {
        get
        {
            short v = 0;
            if (icon && pic)
            {
                v |= STA_OKABLE;
            }
            return v;
        }
    }


    public decimal RealPrice => price - off;

    public bool IsAvailableFor(int mktid)
    {
        return targs == null || targs.Contains(mktid);
    }

    public int StockX => stock / unitx;

    public int AvailX => avail / unitx;

    public StockOp[] Ops => ops;

    public bool IsSpot => typ == TYP_SPOT;

    public bool IsPre => typ == TYP_PRE;

    public override string ToString() => name;
}