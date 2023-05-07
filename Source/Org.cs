using ChainFx;
using ChainFx.Nodal;

namespace ChainSmart;

/// <summary>
/// An organizational unit.
/// </summary>
public class Org : Entity, IKeyable<int>, ITwin
{
    public static readonly Org Empty = new();

    public const short
        TYP_BRD = 0b00000, // brand
        TYP_PRT = 0b01000, // parent
        TYP_RTL = 0b00001, // shop
        TYP_SUP = 0b00010, // source
        TYP_LOG = 0b00100, // logistic
        TYP_MKT = TYP_PRT | TYP_RTL, // market
        TYP_CTR = TYP_PRT | TYP_SUP | TYP_LOG; // center


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
        { STU_CREATED, "新建" },
        { STU_ADAPTED, "修改" },
        { STU_OKED, "上线" },
    };


    // id
    internal int id;

    // parent id, only if shop or source
    internal int prtid;

    // center id, only if market or shop
    internal int ctrid;

    internal string ext; // extended territory name
    internal string legal; // legal name
    internal short regid;
    internal string addr;
    internal double x;
    internal double y;
    internal string tel;
    internal bool trust;
    internal string link;

    internal bool icon;
    internal JObj specs;
    internal bool pic;
    internal bool m1;
    internal bool m2;
    internal bool m3;
    internal bool m4;

    public override void Read(ISource s, short msk = 0xff)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(prtid), ref prtid);
            s.Get(nameof(ctrid), ref ctrid);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(ext), ref ext);
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
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
            s.Get(nameof(m1), ref m1);
            s.Get(nameof(m2), ref m2);
            s.Get(nameof(m3), ref m3);
            s.Get(nameof(m4), ref m4);
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
            if (prtid > 0) s.Put(nameof(prtid), prtid);
            else s.PutNull(nameof(prtid));

            if (ctrid > 0) s.Put(nameof(ctrid), ctrid);
            else s.PutNull(nameof(ctrid));
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(ext), ext);
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
            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
            s.Put(nameof(m1), m1);
            s.Put(nameof(m2), m2);
            s.Put(nameof(m3), m3);
            s.Put(nameof(m4), m4);
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

    public int MarketId => IsMarket ? id : IsOfRetail ? prtid : 0;

    public bool IsParent => (typ & TYP_PRT) == TYP_PRT;

    public bool IsBrand => typ == TYP_BRD;

    public bool IsSupply => typ == TYP_SUP;

    public bool IsOfSupply => (typ & TYP_SUP) == TYP_SUP;

    public bool IsRetail => typ == TYP_RTL;

    public bool IsOfRetail => (typ & TYP_RTL) == TYP_RTL;

    public bool IsMarket => typ == TYP_MKT;

    public bool IsCenter => typ == TYP_CTR;

    public bool HasXy => IsMarket || IsSupply || IsCenter;

    public bool IsTopOrg => prtid == 0;

    public string Name => name;

    private string title;

    public string Title => title ??= IsMarket ? name : '【' + addr + '】' + name;

    public string Ext => ext;

    public int TwinSetKey => prtid;

    public override string ToString() => name;
}