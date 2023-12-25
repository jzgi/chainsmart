using System;
using ChainFX;

namespace ChainSmart;

/// <summary>
/// A carbon credit transaction record.
/// </summary>
public class Carb : IData, IKeyable<(int, DateTime)>
{
    public static readonly Carb Empty = new();

    internal int orgid;
    internal DateTime dt;
    internal int typ;
    internal decimal amt;
    internal short rate;
    internal decimal topay;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(orgid), ref orgid);
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(amt), ref amt);
        s.Get(nameof(rate), ref rate);
        s.Get(nameof(topay), ref topay);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(orgid), orgid);
        s.Put(nameof(dt), dt);
        s.Put(nameof(typ), typ);
        s.Put(nameof(amt), amt);
        s.Put(nameof(rate), rate);
        s.Put(nameof(topay), topay);
    }

    public (int, DateTime) Key => (orgid, dt);
}