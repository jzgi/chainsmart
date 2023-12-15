using System;
using System.Threading;
using ChainFX;
using ChainFX.Nodal;

namespace ChainSmart;

/// <summary>
/// An organizational unit.
/// </summary>
public class Org : Entity, ITwin<int>
{
    public static readonly Org Empty = new();

    public const short
        TYP_UPR = 0b01000, // upper
        TYP_RTL = 0b00001, // retail
        TYP_SUP = 0b00010, // supply
        TYP_LOG = 0b00100, // logistic
        TYP_MKT = TYP_UPR | TYP_RTL, // market
        TYP_CTR = TYP_UPR | TYP_SUP | TYP_LOG; // center


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

    public int MarketId => IsMarket ? id : AsRetail ? upperid : 0;

    public int CenterId => IsCenter ? id : AsSupply ? upperid : 0;

    public bool AsUpper => (typ & TYP_UPR) == TYP_UPR;

    public bool IsMisc => regid == Reg.MISC_REGID;

    //
    // public bool AsService => regid == Reg.SVC_REGID || IsMarket;
    //
    public bool IsSupply => typ == TYP_SUP;

    public bool AsSupply => (typ & TYP_SUP) == TYP_SUP;

    public bool IsRetail => typ == TYP_RTL;

    public bool AsRetail => (typ & TYP_RTL) == TYP_RTL;

    public bool IsMarket => typ == TYP_MKT;

    public bool IsCenter => typ == TYP_CTR;

    public bool IsShop => IsRetail || IsSupply;

    public bool Orderable => bankacct != null && bankacctname != null;

    public bool HasXy => IsMarket || IsSupply || IsCenter;

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

    public string No => IsRetail ? addr : null;

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