using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// An account payable record.
/// </summary>
public class Ap : IData, IKeyable<int>
{
    public static readonly Ap Empty = new();

    public const short
        LVL_BIZ = 1,
        LVL_FEE = 2;

    public static readonly Map<short, string> Levels = new()
    {
        { LVL_BIZ, "结款" },
        { LVL_FEE, "扣费" },
    };

    internal short level;
    internal int orgid;
    internal DateTime dt;
    internal short trans;
    internal decimal amt;
    internal short rate;
    internal decimal topay;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(level), ref level);
        s.Get(nameof(orgid), ref orgid);
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(trans), ref trans);
        s.Get(nameof(amt), ref amt);
        s.Get(nameof(rate), ref rate);
        s.Get(nameof(topay), ref topay);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(level), level);
        s.Put(nameof(orgid), orgid);
        s.Put(nameof(dt), dt);
        s.Put(nameof(trans), trans);
        s.Put(nameof(amt), amt);
        s.Put(nameof(rate), rate);
        s.Put(nameof(topay), topay);
    }

    public int Key => level;
}