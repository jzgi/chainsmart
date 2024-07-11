using System;
using ChainFX;
using static ChainSmart.FinanceUtility;

namespace ChainSmart;

/// <summary>
/// A retail buy record..
/// </summary>
public class Buy : Entity, IKeyable<long>
{
    public static readonly Buy Empty = new();

    public const short
        TYP_ONL = 1,
        TYP_POS = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_ONL, "网售" },
        { TYP_POS, "终端" },
    };

    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, "撤销" },
        { STU_CREATED, "新建" },
        { STU_ADAPTED, "收单" },
        { STU_OKED, "派发" },
        { STU_CLOSED, "关闭" },
    };


    internal int id;
    internal int orgid;
    internal int mktid;
    internal int uid;
    internal string uname;
    internal string utel;
    internal string uarea; // delivery area
    internal string uaddr; // address
    internal string uim;
    internal BuyLn[] lns; // lines
    internal short mode;
    internal decimal fee;
    internal decimal topay;
    internal decimal pay;
    internal decimal ret;
    internal decimal refund;
    internal string refunder;

    public Buy()
    {
    }

    public Buy(User prin, Org shp, BuyLn[] arr)
    {
        typ = TYP_ONL;
        name = shp.name;
        tip = shp.No;
        orgid = shp.id;
        mode = shp.mode;
        mktid = shp.MktId;
        lns = arr;
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
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(mktid), ref mktid);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uarea), ref uarea);
            s.Get(nameof(uaddr), ref uaddr);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(lns), ref lns);
            s.Get(nameof(mode), ref mode);
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
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(mktid), mktid);

            if (uid > 0) s.Put(nameof(uid), uid);
            else s.PutNull(nameof(uid));

            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uarea), uarea);
            s.Put(nameof(uaddr), uaddr);
            s.Put(nameof(uim), uim);
            s.Put(nameof(lns), lns);
            s.Put(nameof(mode), mode);
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
        if (lns != null)
        {
            foreach (var ln in lns)
            {
                sum += ln.SubTotal;
            }
        }

        // set the topay field
        topay = sum;
    }


    public void SetupFeeAndTopay(Org org, string area)
    {
        var sum = 0.00M;
        if (lns != null)
        {
            foreach (var ln in lns)
            {
                sum += ln.SubTotal;
            }
        }

        // compute fee
        //
        if (org.IsStyleDlv)
        {
            var (min, rate, max) = mktdlvfee;
            var feev = Math.Max(min + sum * rate, max);
            feev -= feev % 0.5M;

            // adjust
            var specs = org.specs;
            for (int i = 0; i < specs?.Count; i++)
            {
                var spec = specs.EntryAt(i);
                var v = spec.Value;
                if (v.IsObject)
                {
                    var sub = (JObj)v;
                    for (int k = 0; k < sub.Count; k++)
                    {
                        var e = sub.EntryAt(k);
                        if (e.Key == area && e.Value.IsNumber)
                        {
                            feev += (int)e.Value;
                        }
                    }
                }
                else
                {
                    if (spec.Key == area && spec.Value.IsNumber)
                    {
                        feev += (int)spec.Value;
                    }
                }
            }
            fee = feev;
        }

        topay = sum + fee;
    }


    public long Key => id;


    // STATE
    //

    public bool IsOnline => typ == TYP_ONL;

    public bool IsOnPos => typ == TYP_POS;

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