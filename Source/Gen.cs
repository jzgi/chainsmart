using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// An entry of report generation.
/// </summary>
public class Gen : IData, IKeyable<short>
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

    internal short id;

    internal short typ;

    internal DateTime till;

    internal DateTime started;

    internal DateTime ended;

    internal string opr;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(id), ref id);
        s.Get(nameof(till), ref till);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(started), ref started);
        s.Get(nameof(ended), ref ended);
        s.Get(nameof(opr), ref opr);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(id), id);
        s.Put(nameof(till), till);
        s.Put(nameof(typ), typ);
        s.Put(nameof(started), started);
        s.Put(nameof(ended), ended);
        s.Put(nameof(opr), opr);
    }

    public short Key => id;
}