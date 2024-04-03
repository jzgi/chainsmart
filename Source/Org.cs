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
        TYP_ADM = 0b10000000; // admin

    public const short
        TYP_BIZ_ = 0b000001, // biz
        TYP_BCK_ = 0b000010, // backing
        TYP_MKT_ = 0b000100, // retail
        TYP_SUP_ = 0b001000, // supply
        TYP_EST_ = 0b010000, // establishment
        //
        TYP_SHP = TYP_MKT_ | TYP_BIZ_, // shop
        TYP_STL = TYP_MKT_ | TYP_BIZ_ | TYP_BCK_, // stall
        TYP_MKT = TYP_EST_ | TYP_STL, // market
        //
        TYP_SUP = TYP_SUP_ | TYP_BIZ_, // supplier
        TYP_SRC = TYP_SUP_ | TYP_BCK_, // source
        TYP_SUPSRC = TYP_SUP_ | TYP_BIZ_ | TYP_BCK_, // supply & source
        TYP_HUB = TYP_EST_ | TYP_SUP; // hub

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_SHP, "门店" },
        { TYP_STL, "商户" },
        { TYP_MKT, "市场" },
        { TYP_SRC, "产源" },
        { TYP_SUP, "供应" },
        { TYP_SUPSRC, "产供" },
        { TYP_HUB, "云仓" },
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


    public const short
        MOD_SLF = 0b000001, // self 
        MOD_DLV = 0b000010, // delivery 
        MOD_SVC = 0b000100, // service 
        MOD_CTR = 0b001000 | MOD_DLV, // center
        MOD_CPX = 0b010000 | MOD_DLV; // complex 


    public static readonly Map<short, string> Modes = new()
    {
        { MOD_SLF, "自理模式" },
        { MOD_DLV, "延伸派送模式" },
        { MOD_SVC, "延伸服务模式" },
        { MOD_CTR, "邻里中心模式" },
        { MOD_CPX, "农贸综合体模式" },
    };


    // id
    internal int id;

    // parent id, if mkt or sup
    internal int parentid;

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
    internal string bankacct;
    internal string bankacctname;
    internal JObj specs;

    internal TimeSpan openat;
    internal TimeSpan closeat;
    internal short rank; // credit level
    internal short mode;
    internal short cattyp;
    internal short symtyp;
    internal short tagtyp;
    internal short envtyp;
    internal int[] ties; // ties to other orgs


    internal bool icon;
    internal bool pic;
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
                s.Get(nameof(cover), ref cover);
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
                s.Get(nameof(cattyp), ref cattyp);
                s.Get(nameof(symtyp), ref symtyp);
                s.Get(nameof(tagtyp), ref tagtyp);
                s.Get(nameof(envtyp), ref envtyp);
                s.Get(nameof(ties), ref ties);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
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
                s.Put(nameof(cover), cover);
                s.Put(nameof(legal), legal);
                if (regid > 0) s.Put(nameof(regid), regid);
                else s.PutNull(nameof(regid));
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
                s.Put(nameof(cattyp), cattyp);
                s.Put(nameof(symtyp), symtyp);
                s.Put(nameof(tagtyp), tagtyp);
                s.Put(nameof(envtyp), envtyp);
                s.Put(nameof(ties), ties);
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
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

    public bool AsBiz => (typ & TYP_BIZ_) == TYP_BIZ_;

    public bool AsEst => (typ & TYP_EST_) == TYP_EST_;

    public bool AsMkt => (typ & TYP_MKT_) == TYP_MKT_;

    public bool AsSup => (typ & TYP_SUP_) == TYP_SUP_;

    public int MktId => IsMkt ? id : AsMkt ? parentid : 0;

    public int HubId => IsHub ? id : AsSup ? parentid : 0;

    public bool IsMkt => (typ & TYP_MKT) == TYP_MKT;

    public bool IsHub => (typ & TYP_HUB) == TYP_HUB;

    public bool IsHomeOrg => regid == 0;

    public bool IsSrc => (typ & TYP_SRC) == TYP_SRC;

    public bool IsSup => (typ & TYP_SUP) == TYP_SUP;

    public bool Orderable => bankacct != null && bankacctname != null;

    public bool HasXy => IsMkt || AsSup || IsHub;

    public bool IsTopOrg => parentid == 0;

    public bool IsChildOrg => parentid > 0;

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
                Interlocked.CompareExchange(ref title, string.IsNullOrEmpty(tel) ? name : name + "&nbsp;<a class=\"uk-icon-button uk-light uk-circle\" href=\"tel:" + tel + "\">☏</a>", null);
            }
            return title;
        }
    }

    public string No => AsMkt ? addr : null;

    public string Cover => cover;

    public int ForkKey => parentid;

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


    public static bool IsNonEstSupTyp(short t) => (t & TYP_SUP_) == TYP_SUP_ && t != TYP_HUB;
}