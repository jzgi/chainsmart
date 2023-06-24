using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// A record of accounting generation.
/// </summary>
public class Gen : IData, IKeyable<DateTime>
{
    public static readonly Gen Empty = new();

    public const short
        BUY = 1,
        PUR = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "市场" },
        { 2, "供应" },
    };

    // order date till (since last generation)
    internal DateTime till;

    internal DateTime last;

    internal DateTime started;

    internal DateTime ended;

    internal string opr;

    internal decimal amt;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(till), ref till);
        s.Get(nameof(last), ref last);
        s.Get(nameof(started), ref started);
        s.Get(nameof(ended), ref ended);
        s.Get(nameof(opr), ref opr);
        s.Get(nameof(amt), ref amt);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(till), till);
        s.Put(nameof(last), last);
        s.Put(nameof(started), started);
        s.Put(nameof(ended), ended);
        s.Put(nameof(opr), opr);
        s.Put(nameof(amt), amt);
    }

    public DateTime Key => till;
}