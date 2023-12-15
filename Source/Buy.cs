using ChainFX;

namespace ChainSmart;

/// <summary>
/// A retail buy record..
/// </summary>
public class Buy : Entity, IKeyable<long>
{
    public static readonly Buy Empty = new();

    public const short
        TYP_ORDR = 1,
        TYP_CASH = 2,
        TYP_OTHR = 3;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_ORDR, "网售" },
        { TYP_CASH, "现金" },
        { TYP_OTHR, "其他" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤销" },
        { STU_CREATED, "收单" },
        { STU_ADAPTED, "合单" },
        { STU_OKED, "派发" },
        { STU_CLOSED, "关闭" },
    };


    internal int id;
    internal int rtlid;
    internal int mktid;
    internal int uid;
    internal string uname;
    internal string utel;
    internal string ucom; // community
    internal string uaddr; // address
    internal string uim;
    internal BuyItem[] items; // item lines
    internal decimal fee;
    internal decimal topay;
    internal decimal pay;
    internal decimal ret;
    internal decimal refund;
    internal string refunder;

    public Buy()
    {
    }

    public Buy(User prin, Org rtl, BuyItem[] arr)
    {
        typ = TYP_ORDR;
        name = rtl.name;
        tip = rtl.No;
        rtlid = rtl.id;
        mktid = rtl.MarketId;
        items = arr;
        uid = prin.id;
        uname = prin.name;
        utel = prin.tel;
        uim = prin.im;
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
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(ucom), ref ucom);
            s.Get(nameof(uaddr), ref uaddr);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(items), ref items);
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

            if (uid > 0) s.Put(nameof(uid), uid);
            else s.PutNull(nameof(uid));

            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(ucom), ucom);
            s.Put(nameof(uaddr), uaddr);
            s.Put(nameof(uim), uim);
            s.Put(nameof(items), items);
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

    public void InitTopay()
    {
        var sum = fee;
        if (items != null)
        {
            foreach (var ln in items)
            {
                sum += ln.SubTotal;
            }
        }

        // set the topay field
        topay = sum;
    }

    public long Key => id;


    // STATE
    //

    public bool IsFromNet => typ == TYP_ORDR;

    public bool IsOnPos => typ == TYP_CASH;

    public override string ToString() => uname + "购买" + name + "商品";

    public const short
        STA_CANCELLABLE = 1,
        STA_REVERSABLE = 2;

    public override short ToState()
    {
        short v = 0;
        if (oker != null || adapter != null)
        {
            v |= STA_REVERSABLE;
        }
        return v;
    }

    public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
}