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
        TYP_HUB = 1,
        TYP_SRC = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_HUB, "云仓" },
        { TYP_SRC, "产源" },
    };

    public static readonly Map<short, string> Ranks = new()
    {
        { 0, null },
        { 1, "普标" },
        { 2, "高标" },
        { 4, "顶级" },
    };

    internal int id;
    internal int orgid;
    internal short cattyp;
    internal DateTime shipon;
    internal int srcid;
    internal string unit;
    internal short unitw;
    internal short unitx;
    internal decimal price;
    internal decimal off;

    internal int min;
    internal int max;

    // traceability
    internal int cap;
    internal int nstart;
    internal int nend;
    internal string linka;
    internal string linkb;

    // media
    internal bool icon;
    internal bool pic;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool m4;

    internal StockOp[] ops;

    // EXTRA: of either the lots or the lotinvs table
    internal int stock;

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
            s.Get(nameof(srcid), ref srcid);
            s.Get(nameof(cattyp), ref cattyp);
            s.Get(nameof(shipon), ref shipon);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitw), ref unitw);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(cap), ref cap);
            s.Get(nameof(nstart), ref nstart);
            s.Get(nameof(nend), ref nend);
            s.Get(nameof(linka), ref linka);
            s.Get(nameof(linkb), ref linkb);
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
            s.Get(nameof(m1), ref m1);
            s.Get(nameof(m2), ref m2);
            s.Get(nameof(m3), ref m3);
            s.Get(nameof(m4), ref m4);
        }

        if ((msk & MSK_AUX) == MSK_AUX)
        {
            s.Get(nameof(ops), ref ops);
        }
        if ((msk & MSK_EXTRA) == MSK_EXTRA)
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
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            if (srcid > 0) s.Put(nameof(srcid), srcid); else s.PutNull(nameof(srcid));
            s.Put(nameof(cattyp), cattyp);
            s.Put(nameof(shipon), shipon);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitw), unitw);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(cap), cap);
            s.Put(nameof(nstart), nstart);
            s.Put(nameof(nend), nend);
            s.Put(nameof(linka), linka);
            s.Put(nameof(linkb), linkb);

            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
            s.Put(nameof(m1), m1);
            s.Put(nameof(m2), m2);
            s.Put(nameof(m3), m3);
            s.Put(nameof(m4), m4);
        }

        if ((msk & MSK_AUX) == MSK_AUX)
        {
            s.Put(nameof(ops), ops);
        }
        if ((msk & MSK_EXTRA) == MSK_EXTRA)
        {
            s.Put(nameof(stock), stock);
        }
    }

    public int Key => id;

    // STATE
    //

    public const short STA_OKABLE = 1;

    public override short ToState()
    {
        short v = 0;
        if (icon && pic)
        {
            v |= STA_OKABLE;
        }
        return v;
    }


    public decimal RealPrice => price - off;

    public int StockX => stock / unitx;

    public StockOp[] Ops => ops;

    public bool IsOnHub => typ == TYP_HUB;

    public bool IsOnSrc => typ == TYP_SRC;

    public override string ToString() => name;


    public bool TryGetStockOp(int offset, out StockOp value)
    {
        if (ops != null)
        {
            var num = offset;
            for (int i = 0; i < ops.Length; i++)
            {
                var op = ops[i];
                num -= op.qty;
                if (num <= 0)
                {
                    value = op;
                    return true;
                }
            }
        }
        value = default;
        return false;
    }
}