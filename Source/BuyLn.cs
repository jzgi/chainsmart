﻿using ChainFX;

namespace ChainSmart;

/// <summary>
/// A line item of buy record..
/// </summary>
public class BuyLn : IData, IKeyable<int>
{
    public int itemid;

    public string name;

    public string unit; // basic unit

    public string unitip;

    public decimal price;

    public decimal off;

    public decimal qty;

    public BuyLn()
    {
    }

    public BuyLn(int itemid, short qty)
    {
        this.itemid = itemid;
        this.qty = qty;
    }

    /// <summary>
    /// To construct a POS buy record.
    /// </summary>
    /// <param name="itemid"></param>
    /// <param name="comp"></param>
    public BuyLn(int itemid, string[] comp)
    {
        this.itemid = itemid;

        name = comp[0];
        unit = comp[1];
        unitip = comp[2];
        price = decimal.Parse(comp[3]);
        qty = decimal.Parse(comp[4]);
    }

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(itemid), ref itemid);
        s.Get(nameof(name), ref name);
        s.Get(nameof(unit), ref unit);
        s.Get(nameof(unitip), ref unitip);
        s.Get(nameof(price), ref price);
        s.Get(nameof(off), ref off);
        s.Get(nameof(qty), ref qty);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(itemid), itemid);
        s.Put(nameof(name), name);
        s.Put(nameof(unit), unit);
        s.Put(nameof(unitip), unitip);
        s.Put(nameof(price), price);
        s.Put(nameof(off), off);
        s.Put(nameof(qty), qty);
    }

    public int Key => itemid;

    public decimal RealPrice => price - off;

    public decimal SubTotal => decimal.Round(RealPrice * qty, 2);

    internal void Init(Item m, bool vip)
    {
        name = m.name;
        unit = m.unit;
        unitip = m.unitip;
        price = m.price;

        if (vip)
        {
            off = m.off;
        }
    }
}