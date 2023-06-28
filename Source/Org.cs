using System;
using System.Threading;
using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/// <summary>
/// An organizational unit.
/// </summary>
public class Org : Entity, ITwin<int>
{
    public static readonly Org Empty = new();

    public const short
        TYP_BRD = 0b00000, // brand
        TYP_UPR = 0b01000, // upper
        TYP_RTL = 0b00001, // shop
        TYP_SUP = 0b00010, // source
        TYP_LOG = 0b00100, // logistic
        TYP_MKT = TYP_UPR | TYP_RTL, // market
        TYP_CTR = TYP_UPR | TYP_SUP | TYP_LOG; // center


    public const short
        STA_VOID = 0,
        STA_PRE = 1,
        STA_FINE = 2,
        STA_TOP = 4;

    public static readonly Map<short, string> States = new()
    {
        { STA_VOID, "停业" },
        { STA_PRE, "放假" },
        { STA_FINE, "正常" },
        { STA_TOP, "满负" },
    };


    public new static readonly Map<short, string> Statuses = new()
    {
        { STU_VOID, null },
        { STU_CREATED, "新建" },
        { STU_ADAPTED, "修改" },
        { STU_OKED, "上线" },
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
    internal string link;

    internal TimeSpan opened;
    internal TimeSpan closed;

    internal short credit;
    internal string bankacct;
    internal string bankacctname;

    internal bool icon;
    internal JObj specs;
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
                s.Get(nameof(link), ref link);
                s.Get(nameof(specs), ref specs);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(opened), ref opened);
                s.Get(nameof(closed), ref closed);
                s.Get(nameof(credit), ref credit);
                s.Get(nameof(bankacct), ref bankacct);
                s.Get(nameof(bankacctname), ref bankacctname);
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
                s.Put(nameof(link), link);
                s.Put(nameof(specs), specs);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(opened), opened);
                s.Put(nameof(closed), closed);
                s.Put(nameof(credit), credit);
                s.Put(nameof(bankacct), bankacct);
                s.Put(nameof(bankacctname), bankacctname);
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

    public const short STA_OKABLE = 1;

    public override short State
    {
        get
        {
            short v = 0;
            if (icon && pic)
            {
                v |= STA_OKABLE;
            }
            return v;
        }
    }


    public string Tel => tel;

    public int ThisMarketId => IsMarket ? id : OfRetail ? upperid : 0;

    public int ThisCenterId => IsCenter ? id : OfSupply ? upperid : 0;

    public bool OfUpper => (typ & TYP_UPR) == TYP_UPR;

    public bool IsBrand => typ == TYP_BRD;

    public bool IsSupply => typ == TYP_SUP;

    public bool OfSupply => (typ & TYP_SUP) == TYP_SUP;

    public bool IsRetail => typ == TYP_RTL;

    public bool OfRetail => (typ & TYP_RTL) == TYP_RTL;

    public bool IsMarket => typ == TYP_MKT;

    public bool IsCenter => typ == TYP_CTR;

    public bool HasXy => IsMarket || IsSupply || IsCenter;

    public bool IsTopOrg => upperid == 0;

    public string Name => name;

    private string title;

    public string Title => title ??= IsMarket ? name : name + '（' + addr + '）';

    public string Cover => cover;

    public int SetKey => upperid;

    public bool IsOpen(TimeSpan now)
    {
        return IsOked && now > opened && now < closed;
    }

    public override string ToString() => name;


    // EVENT 


    private OrgNoticePack notices;

    public OrgNoticePack Notices
    {
        get
        {
            if (notices == null)
            {
                Interlocked.CompareExchange(ref notices, new OrgNoticePack(), null);
            }
            return notices;
        }
    }

    private OrgEventPack events;

    public OrgEventPack Events
    {
        get
        {
            if (events == null)
            {
                Interlocked.CompareExchange(ref events, new OrgEventPack(), null);
            }
            return events;
        }
    }
}