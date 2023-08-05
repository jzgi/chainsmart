using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// A supply purchase record.
/// </summary>
public class Pur : Entity, IKeyable<int>
{
    public static readonly Pur Empty = new();

    public const short
        TYP_HUB = 1,
        TYP_SRC = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_HUB, "品控仓" },
        { TYP_SRC, "产源" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤销" },
        { STU_CREATED, "收单" },
        { STU_ADAPTED, "发货" },
        { STU_OKED, "收货" },
        { STU_CLOSED, "关闭" },
    };


    internal int id;

    internal int rtlid; // retail
    internal int mktid; // market
    internal int hubid; // hub warehouse
    internal int supid; // supply
    internal int ctrid; // info center

    internal int lotid;

    internal string unit;
    internal short unitw;
    internal short unitx;
    internal decimal price;
    internal decimal off;
    internal int qty;
    internal decimal fee; // transport fee
    internal decimal topay;
    internal decimal pay;
    internal int ret; // qty cut
    internal decimal refund; // pay refunded
    internal string refunder;


    public Pur()
    {
    }

    public Pur(Lot lot, Org rtl, Org sup)
    {
        typ = lot.typ;
        name = lot.name;
        tip = lot.tip;

        rtlid = rtl.id;
        mktid = rtl.MarketId;
        hubid = rtl.hubid;
        supid = sup.id;
        ctrid = sup.CenterId;

        lotid = lot.id;
        unit = lot.unit;
        unitw = lot.unitw;
        unitx = lot.unitx;
        price = lot.price;
        off = lot.off;
    }

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(rtlid), ref rtlid);
            s.Get(nameof(mktid), ref mktid);
            s.Get(nameof(hubid), ref hubid);
            s.Get(nameof(supid), ref supid);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(lotid), ref lotid);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitw), ref unitw);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(fee), ref fee);
            s.Get(nameof(topay), ref topay);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(ret), ref ret);
            s.Get(nameof(refund), ref refund);
            s.Get(nameof(refunder), ref refunder);
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
            s.Put(nameof(rtlid), rtlid);
            s.Put(nameof(mktid), mktid);
            s.Put(nameof(hubid), hubid);
            s.Put(nameof(supid), supid);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(lotid), lotid);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitw), unitw);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(fee), fee);
            s.Put(nameof(topay), topay);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(pay), pay);
            s.Put(nameof(ret), ret);
            s.Put(nameof(refund), refund);
            s.Put(nameof(refunder), refunder);
        }
    }

    public int Key => id;

    // STATE
    //

    public const short STA_CANCELL = 1;

    public override short ToState()
    {
        var now = DateTime.Now;
        short v = 0;
        if (now.Date <= created.Date.AddDays(1) && now.Hour < 12)
        {
            v |= STA_CANCELL;
        }
        return v;
    }


    public short QtyX => (short)(qty / unitx);

    public decimal RealPrice => price - off;

    public decimal Total => RealPrice * qty;

    public override string ToString() => name;

    public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
}