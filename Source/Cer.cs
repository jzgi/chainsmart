using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// A carbon emission reduction deal.
/// </summary>
public class Cer : IData, IKeyable<(int, DateTime)>
{
    public static readonly Cer Empty = new();

    internal int userid;
    internal DateTime dt;
    internal int typ;
    internal decimal v;
    internal string opr;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(userid), ref userid);
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(v), ref v);
        s.Get(nameof(opr), ref opr);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(userid), userid);
        s.Put(nameof(dt), dt);
        s.Put(nameof(typ), typ);
        s.Put(nameof(v), v);
        s.Put(nameof(opr), opr);
    }

    public (int, DateTime) Key => (userid, dt);
}