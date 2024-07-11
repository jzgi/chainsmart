using System;
using System.Threading;
using ChainFX;
using ChainFX.Nodal;
using ChainFX.Web;

namespace ChainSmart;

/// <summary>
/// An organizational unit record.
/// </summary>
public class Org : Entity, ITwin<int>, IFolderable
{
    public static readonly Org Empty = new();

    public const short
        TYP_ADM = 0b10000000; // admin

    // type constants
    public const short
        TYP_SAL_ = 0b000001, // sale
        TYP_BCK_ = 0b000010, // backing
        TYP_RTL_ = 0b000100, // retail
        TYP_WHL_ = 0b001000, // wholesale
        TYP_EST_ = 0b010000, // establishment
        //
        TYP_SHP = TYP_RTL_ | TYP_SAL_, // shop
        TYP_SHV = TYP_RTL_ | TYP_BCK_, // shop virtual
        TYP_SHX = TYP_RTL_ | TYP_SAL_ | TYP_BCK_, // shop + purchase 
        TYP_MKV = TYP_EST_ | TYP_SHV, // market virtual
        TYP_MKT = TYP_EST_ | TYP_SHX, // market
        //
        TYP_SRC = TYP_WHL_ | TYP_BCK_, // source
        TYP_SUP = TYP_WHL_ | TYP_SAL_ | TYP_BCK_, // supply
        TYP_HUB = TYP_EST_ | TYP_SUP; // hub

    // type definitions
    public static readonly Map<short, string> Typs = new()
    {
        { TYP_SHP, "门店" },
        { TYP_SHV, "泛商户" },
        { TYP_SHX, "商户" },
        { TYP_MKV, "泛市场" },
        { TYP_MKT, "市场" },
        { TYP_SRC, "产源" },
        { TYP_SUP, "供应" },
        { TYP_HUB, "品控云仓" },
    };

    public static readonly Map<short, string> Ranks = new()
    {
        { 0, null },
        { 1, "★" },
        { 2, "★★" },
        { 3, "★★★" },
        { 4, "★★★★" },
        { 5, "★★★★★" },
    };


    // delivery style constants
    public const short
        MOD_SLFDLV = 0, // self-handling 
        MOD_MIXDLV = 1;

    // style definitions
    public static readonly Map<short, string> Modes = new()
    {
        { MOD_SLFDLV, "自理派发" },
        { MOD_MIXDLV, "合单统一派发" },
    };


    // id
    internal int id;

    // parent id, if mkt or sup
    internal int parentid;

    // connected hub warehouse id, if market or retail 
    internal int hubid;

    internal string whole; // name as the whole
    internal string wholetip;
    internal string legal; // legal name
    internal short regid;
    internal string addr;
    internal double x;
    internal double y;
    internal string tel;
    internal bool trust;
    internal string bankacct;
    internal string bankacctname;
    internal JObj specs;

    internal TimeSpan openat;
    internal TimeSpan closeat;
    internal short rank; // credit level
    internal short mode;

    internal bool icon;
    internal bool pic;
    internal bool img;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool m4;


    public override void Read(ISource s, short msk = 0xff)
    {
        lock (this)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(parentid), ref parentid);
                s.Get(nameof(hubid), ref hubid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(mode), ref mode);
                s.Get(nameof(rank), ref rank);
                s.Get(nameof(whole), ref whole);
                s.Get(nameof(wholetip), ref wholetip);
                s.Get(nameof(legal), ref legal);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(trust), ref trust);
                s.Get(nameof(specs), ref specs);
                s.Get(nameof(bankacctname), ref bankacctname);
                s.Get(nameof(bankacct), ref bankacct);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(openat), ref openat);
                s.Get(nameof(closeat), ref closeat);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
                s.Get(nameof(img), ref img);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(m4), ref m4);
            }
        }
    }

    public override void Write(ISink s, short msk = 0xff)
    {
        lock (this)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                if (parentid > 0) s.Put(nameof(parentid), parentid);
                else s.PutNull(nameof(parentid));

                if (hubid > 0) s.Put(nameof(hubid), hubid);
                else s.PutNull(nameof(hubid));
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(mode), mode);
                s.Put(nameof(rank), rank);
                s.Put(nameof(whole), whole);
                s.Put(nameof(wholetip), wholetip);
                s.Put(nameof(legal), legal);
                if (regid <= 0 && !IsShp) s.PutNull(nameof(regid));
                else s.Put(nameof(regid), regid);
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(trust), trust);
                s.Put(nameof(tel), tel);
                s.Put(nameof(specs), specs);
                s.Put(nameof(bankacctname), bankacctname);
                s.Put(nameof(bankacct), bankacct);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(openat), openat);
                s.Put(nameof(closeat), closeat);
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(img), img);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
            }
        }
    }


    public int Key => id;

    public short Idx => rank;

    public short Style => mode;

    // STATE
    //

    public const short
        STA_OKABLE = 1,
        STA_AAPLUS = 2;

    public override short ToState()
    {
        short v = 0;
        if (icon && pic)
        {
            v |= STA_OKABLE;
        }
        if (rank >= 5)
        {
            v |= STA_AAPLUS;
        }
        return v;
    }

    public string Tel => tel;

    public bool AsFrt => (typ & TYP_SAL_) == TYP_SAL_;

    public bool AsEst => (typ & TYP_EST_) == TYP_EST_;

    public bool AsRtl => (typ & TYP_RTL_) == TYP_RTL_;

    public bool AsWhl => (typ & TYP_WHL_) == TYP_WHL_;

    public int MktId => IsRtlEst ? id : AsRtl ? parentid : 0;

    public int HubId => IsHub ? id : AsWhl ? parentid : 0;

    public bool IsMkv => (typ & TYP_MKV) == TYP_MKV;

    public bool IsMkt => (typ & TYP_MKT) == TYP_MKT;

    public bool IsRtlEst => (typ & (TYP_RTL_ | TYP_EST_)) == (TYP_RTL_ | TYP_EST_);

    public bool IsShp => (typ & TYP_SHP) == TYP_SHP;

    public bool IsShv => (typ & TYP_SHV) == TYP_SHV;

    public bool IsShx => (typ & TYP_SHX) == TYP_SHX;

    public bool IsHub => (typ & TYP_HUB) == TYP_HUB;

    public bool IsHomeOrg => regid == 0;

    public bool IsSrc => (typ & TYP_SRC) == TYP_SRC;

    public bool IsSup => (typ & TYP_SUP) == TYP_SUP;

    public bool HasXy => IsRtlEst || AsWhl || IsHub;

    public bool IsTopOrg => parentid == 0;

    public bool IsChildOrg => parentid > 0;

    public bool IsLink => addr?.IndexOf('/') >= 0;

    public bool IsOrdinary => !IsTopOrg && !IsLink;

    public string Name => name;


    public bool IsStyleSlf => (mode & MOD_SLFDLV) == MOD_SLFDLV;

    public bool IsStyleDlv => (mode & MOD_MIXDLV) == MOD_MIXDLV;

    public string No => IsMkt ? null : addr;

    public string Whole => whole;

    public string WholeName => whole ?? name;

    public int ForkKey => parentid;


    public bool Payable => !string.IsNullOrEmpty(bankacct) && !string.IsNullOrEmpty(bankacctname);

    public bool Workable => status > 1;

    public bool Openable(TimeSpan now)
    {
        return IsOked && now > openat && now < closeat;
    }

    public override string ToString() => name;


    //
    // watch set

    private WatchSet watchset;

    public WatchSet WatchSet
    {
        get
        {
            if (watchset == null)
            {
                Interlocked.CompareExchange(ref watchset, new WatchSet(), null);
            }
            return watchset;
        }
    }


    //
    // buy pack for extern 

    private BuySet buyset;

    public BuySet BuySet
    {
        get
        {
            if (buyset == null)
            {
                Interlocked.CompareExchange(ref buyset, new BuySet(), null);
            }
            return buyset;
        }
    }


    public static bool IsNonEstSupTyp(short t) => (t & TYP_WHL_) == TYP_WHL_ && t != TYP_HUB;
}