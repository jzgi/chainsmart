using ChainFX;

namespace ChainSmart;

/// <summary>
/// A retail item record.
/// </summary>
public class Item : Entity, IKeyable<int>
{
    public static readonly Item Empty = new();

    public const short
        TYP_NEW = 1,
        TYP_IMP = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { TYP_NEW, "创建" },
        { TYP_IMP, "导入" },
    };


    public static readonly Map<short, string> Ranks = new()
    {
        { 0, null },
        { 1, "普标" },
        { 2, "高标" },
        { 4, "顶级" },
    };

    internal int id;
    internal int orgid;
    internal int lotid;
    internal short rank;
    internal string unit;
    internal short unitw;
    internal decimal price;
    internal decimal off;
    internal bool promo;
    internal short step;
    internal short max;
    internal short min;
    internal short stock;

    internal bool icon;
    internal bool pic;

    internal StockOp[] ops;

    public override void Read(ISource s, short msk = 255)
    {
        base.Read(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Get(nameof(id), ref id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(lotid), ref lotid);
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(rank), ref rank);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitw), ref unitw);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(promo), ref promo);
            s.Get(nameof(step), ref step);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(stock), ref stock);
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
        }

        if ((msk & MSK_AUX) == MSK_AUX)
        {
            s.Get(nameof(ops), ref ops);
        }
    }

    public override void Write(ISink s, short msk = 255)
    {
        base.Write(s, msk);

        if ((msk & MSK_ID) == MSK_ID)
        {
            s.Put(nameof(id), id);
        }

        if ((msk & MSK_BORN) == MSK_BORN)
        {
            s.Put(nameof(orgid), orgid);
            if (lotid > 0) s.Put(nameof(lotid), lotid);
            else s.PutNull(nameof(lotid));
        }

        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(rank), rank);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitw), unitw);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(promo), promo);
            s.Put(nameof(step), step);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
        }

        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(stock), stock);
            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
        }

        if ((msk & MSK_AUX) == MSK_AUX)
        {
            s.Put(nameof(ops), ops);
        }
    }

    public int Key => id;

    // STATE
    //

    public const short STA_OKABLE = 1;

    public override short ToState()
    {
        short v = 0;
        if (icon && pic)
        {
            v |= STA_OKABLE;
        }
        return v;
    }

    public decimal RealPrice => price - off;

    public decimal GetRealOff(bool vip) => vip || promo ? off : 0;

    public bool IsImported => lotid > 0;

    public StockOp[] Ops => ops;
}