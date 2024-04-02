using ChainFX;

namespace ChainSmart;

/// <summary>
/// An advertizing record. 
/// </summary>
public class Ad : Entity, IKeyable<int>
{
    public static readonly Code Empty = new();

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "特金" },
        { 2, "普金" },
        { 3, "特塑" },
        { 4, "普塑" },
        { 5, "特贴" },
        { 6, "普贴" },
        { 7, "RFID" },
    };


    internal int id;

    internal int orgid;

    internal bool icon;

    internal bool pic;

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
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(name), ref name);
            s.Get(nameof(tip), ref tip);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Get(nameof(icon), ref icon);
            s.Get(nameof(pic), ref pic);
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
        }
        if ((msk & MSK_EDIT) == MSK_EDIT)
        {
            s.Put(nameof(typ), typ);
            s.Put(nameof(name), name);
            s.Put(nameof(tip), tip);
        }
        if ((msk & MSK_LATER) == MSK_LATER)
        {
            s.Put(nameof(icon), icon);
            s.Put(nameof(pic), pic);
        }
    }

    public int Key => id;

    public override string ToString() => name;
}