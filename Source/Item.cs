using System;
using ChainFX;

namespace ChainSmart;

/// <summary>
/// A retail item record.
/// </summary>
public class Item : Entity, IKeyable<int>
{
    public static readonly Item Empty = new();

    public const short
        TYP_MKT = 1,
        TYP_SUP = 4;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_MKT, "市场" },
        { TYP_SUP, "供应" },
    };


    internal int id;
    internal int orgid;

    internal int srcid;
    internal short cat;
    internal string unit;
    internal string unitip;
    internal short unitx;
    internal decimal price;
    internal decimal off;
    internal bool promo;
    internal short min;
    internal short max;
    internal short stock;
    internal string sort;
    internal short sym;
    internal DateTime symed;
    internal string symer;

    internal bool icon;
    internal bool pic;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool m4;

    public override void Read(ISource s, short msk = 255)
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
            s.Get(nameof(cat), ref cat);
            s.Get(nameof(sort), ref sort);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(promo), ref promo);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(stock), ref stock);
            s.Get(nameof(sym), ref sym);
            s.Get(nameof(symed), ref symed);
            s.Get(nameof(symer), ref symer);
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
            s.Get(nameof(m1), ref m1);
            s.Get(nameof(m2), ref m2);
            s.Get(nameof(m3), ref m3);
            s.Get(nameof(m4), ref m4);
        }
    }

    public override void Write(ISink s, short msk = 255)
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
            s.Put(nameof(srcid), srcid);
            s.Put(nameof(cat), cat);
            s.Put(nameof(sort), sort);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(promo), promo);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(stock), stock);
            s.Put(nameof(sym), sym);
            s.Put(nameof(symed), symed);
            s.Put(nameof(symer), symer);
            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
            s.Put(nameof(m1), m1);
            s.Put(nameof(m2), m2);
            s.Put(nameof(m3), m3);
            s.Put(nameof(m4), m4);
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

    public decimal GetRealOff(bool vip) => vip || promo ? off : 0;

    public bool IsImported => srcid > 0;
}