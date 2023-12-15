using ChainFX;

namespace ChainSmart;

/// <summary>
/// An aggregation of orders.
/// </summary>
public class PurAgg : IData, IKeyable<int>
{
    public static readonly PurAgg Empty = new();

    internal string name; // shop

    internal int rtlid; // shop
    internal int rtlname;
    internal int itemid; // shop
    internal int count;
    internal short status;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(name), ref name);
        s.Get(nameof(rtlid), ref rtlid);
        s.Get(nameof(rtlname), ref rtlname);
        s.Get(nameof(count), ref count);
        s.Get(nameof(status), ref status);
    }

    public void Write(ISink s, short msk = 0xff)
    {
    }

    public int Key => rtlid;
}