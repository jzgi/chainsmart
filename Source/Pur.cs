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
        TYP_NORM = 1,
        TYP_ADVC = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_NORM, "现供" },
        { TYP_ADVC, "助农" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤单" },
        { STU_CREATED, "下单" },
        { STU_ADAPTED, "备发" },
        { STU_OKED, "发货" },
    };


    internal int id;

    internal int rtlid; // shop
    internal string rtlname;
    internal int mktid; // market
    internal int ctrid; // center
    internal int supid; // source
    internal string supname;

    internal int lotid;

    internal string unit;
    internal short unitw;
    internal short unitx;
    internal decimal price;
    internal decimal off;
    internal int qty;
    internal decimal topay;
    internal decimal pay;
    internal int ret; // qty cut
    internal decimal refund; // pay refunded


    public Pur()
    {
    }

    public Pur(Lot lot, Org rtl)
    {
        typ = lot.typ;
        name = lot.name;
        tip = lot.tip;

        rtlid = rtl.id;
        rtlname = rtl.Name;
        mktid = rtl.MarketId;
        supid = lot.orgid;
        supname = lot.orgname;
        ctrid = rtl.ctrid;

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
            s.Get(nameof(rtlname), ref rtlname);
            s.Get(nameof(mktid), ref mktid);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(supid), ref supid);
            s.Get(nameof(supname), ref supname);
            s.Get(nameof(lotid), ref lotid);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitw), ref unitw);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(topay), ref topay);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(ret), ref ret);
            s.Get(nameof(refund), ref refund);
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
            s.Put(nameof(rtlname), rtlname);
            s.Put(nameof(mktid), mktid);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(supid), supid);
            s.Put(nameof(supname), supname);
            s.Put(nameof(lotid), lotid);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitw), unitw);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(topay), topay);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(pay), pay);
            s.Put(nameof(ret), ret);
            s.Put(nameof(refund), refund);
        }
    }

    public int Key => id;

    // STATE
    //

    public const short STA_CANCELL = 1;

    public override short State
    {
        get
        {
            var now = DateTime.Now;
            short v = 0;
            if (now.Date <= created.Date.AddDays(1) && now.Hour < 12)
            {
                v |= STA_CANCELL;
            }
            return v;
        }
    }


    public short QtyX => (short)(qty / unitx);

    public decimal RealPrice => price - off;

    public decimal Total => RealPrice * qty;

    public override string ToString() => rtlname + "采购" + supname + "产品" + name;

    public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
}