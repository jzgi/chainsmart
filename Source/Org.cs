using System;
using System.Threading;
using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

/// <summary>
/// An organizational unit record.
/// </summary>
public class Org : Entity, ITwin<int>
{
    public static readonly Org Empty = new();

    public const short
        _BIZ = 0b000001, // biz
        _BCK = 0b000010, // backing
        _UPR = 0b010000, // upper
        TYP_RTL_ = 0b000100, // retail
        TYP_SUP_ = 0b001000, // supply
        //
        TYP_RTL_SAL = TYP_RTL_ | _BIZ,
        TYP_RTL_PUR = TYP_RTL_ | _BCK,
        TYP_RTL_FUL = TYP_RTL_ | _BIZ | _BCK,
        //
        TYP_MKT = _UPR | TYP_RTL_,
        TYP_MKT_FUL = _UPR | TYP_RTL_FUL,
        //
        TYP_SUP_SAL = TYP_SUP_ | _BIZ,
        TYP_SUP_SRC = TYP_SUP_ | _BCK,
        TYP_SUP_FUL = TYP_SUP_ | _BIZ | _BCK,
        //
        TYP_CTR = _UPR | TYP_SUP_, // center
        TYP_CTR_FUL = _UPR | TYP_SUP_FUL;

    public static readonly Map<short, string> RtlTyps = new()
    {
        { TYP_RTL_SAL, "网售主" },
        { TYP_RTL_PUR, "采购主" },
        { TYP_RTL_FUL, "商户" },
    };

    public static readonly Map<short, string> MktTyps = new()
    {
        { TYP_MKT, "市场" },
        { TYP_MKT_FUL, "市场＋自营业务" },
    };

    public static readonly Map<short, string> SupTyps = new()
    {
        { TYP_SUP_SAL, "商户" },
        { TYP_SUP_SRC, "产源" },
        { TYP_SUP_FUL, "产原型商户" },
    };

    public static readonly Map<short, string> CtrTyps = new()
    {
        { TYP_CTR, "品控" },
        { TYP_CTR_FUL, "品控＋自营业务" },
    };


    public static readonly Map<short, string> Ranks = new()
    {
        { 0, null },
        { 1, "B" },
        { 2, "BB" },
        { 3, "BBB" },
        { 4, "A" },
        { 5, "AA" },
        { 6, "AAA" },
    };


    // id
    internal int id;

    // parent id, if rtl or sup
    internal int upperid;

    // connected hub warehouse id, if market or retail 
    internal int hubid;

    internal string cover; // coverage
    internal string legal; // legal name
    internal short regid;
    internal string addr;
    internal double x;
    internal double y;
    internal string tel;
    internal bool trust;
    internal string descr;
    internal string bankacct;
    internal string bankacctname;
    internal JObj specs;

    internal TimeSpan openat;
    internal TimeSpan closeat;
    internal short rank; // credit level
    internal decimal carb; // carbon credits

    internal bool icon;
    internal bool pic;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool scene;

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
                s.Get(nameof(upperid), ref upperid);
                s.Get(nameof(hubid), ref hubid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(cover), ref cover);
                s.Get(nameof(legal), ref legal);
                s.Get(nameof(regid), ref regid);
                s.Get(nameof(addr), ref addr);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(tel), ref tel);
                s.Get(nameof(trust), ref trust);
                s.Get(nameof(descr), ref descr);
                s.Get(nameof(specs), ref specs);
                s.Get(nameof(bankacctname), ref bankacctname);
                s.Get(nameof(bankacct), ref bankacct);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(openat), ref openat);
                s.Get(nameof(closeat), ref closeat);
                s.Get(nameof(rank), ref rank);
                s.Get(nameof(carb), ref carb);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(scene), ref scene);
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
                if (upperid > 0) s.Put(nameof(upperid), upperid);
                else s.PutNull(nameof(upperid));

                if (hubid > 0) s.Put(nameof(hubid), hubid);
                else s.PutNull(nameof(hubid));
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(cover), cover);
                s.Put(nameof(legal), legal);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
                s.Put(nameof(addr), addr);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(trust), trust);
                s.Put(nameof(tel), tel);
                s.Put(nameof(descr), descr);
                s.Put(nameof(specs), specs);
                s.Put(nameof(bankacctname), bankacctname);
                s.Put(nameof(bankacct), bankacct);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(openat), openat);
                s.Put(nameof(closeat), closeat);
                s.Put(nameof(rank), rank);
                s.Put(nameof(carb), carb);
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(scene), scene);
            }
        }
    }


    public int Key => id;

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

    public int MarketId => IsMkt ? id : AsRtl ? upperid : 0;

    public int CenterId => AsCtr ? id : AsSup ? upperid : 0;

    public bool AsUpper => (typ & _UPR) == _UPR;

    public bool IsMisc => regid == Reg.HOME_REGID;

    //
    // public bool AsService => regid == Reg.SVC_REGID || IsMarket;
    //
    public bool IsSup => typ == TYP_SUP_;

    public bool AsSup => (typ & TYP_SUP_) == TYP_SUP_;

    public bool IsSrc => typ == TYP_SUP_SRC;

    public bool AsSrc => (typ & TYP_SUP_SRC) == TYP_SUP_SRC;

    // public bool IsRtl => typ == _RTL;

    public bool AsRtl => (typ & TYP_RTL_) == TYP_RTL_;

    public bool IsMkt => (typ & TYP_MKT) == TYP_MKT;

    public bool AsCtr => (typ & TYP_CTR) == TYP_CTR;

    public bool Orderable => bankacct != null && bankacctname != null;

    public bool HasXy => IsMkt || IsSup || AsCtr;

    public bool IsTopOrg => upperid == 0;

    public bool IsLink => addr?.IndexOf('/') >= 0;

    public string Name => name;

    private string title;

    public string Title
    {
        get
        {
            if (title == null)
            {
                var no = No;
                Interlocked.CompareExchange(ref title, string.IsNullOrEmpty(no) ? name : name + '（' + no + '）', null);
            }
            return title;
        }
    }

    public string No => AsRtl ? addr : null;

    public string Cover => cover;

    public int ForkKey => upperid;

    public bool IsOpen(TimeSpan now)
    {
        return IsOked && now > openat && now < closeat;
    }

    public override string ToString() => name;


    // EVENT 


    private OrgNoticePack noticep;

    public OrgNoticePack NoticePack
    {
        get
        {
            if (noticep == null)
            {
                Interlocked.CompareExchange(ref noticep, new OrgNoticePack(), null);
            }
            return noticep;
        }
    }

    private OrgEventPack eventp;

    public OrgEventPack EventPack
    {
        get
        {
            if (eventp == null)
            {
                Interlocked.CompareExchange(ref eventp, new OrgEventPack(), null);
            }
            return eventp;
        }
    }
}