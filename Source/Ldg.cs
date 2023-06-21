using System;
using ChainFx;

namespace ChainSmart;

/// <summary>
/// A ledger entry record.
/// </summary>
public struct Ldg : IData
{
    public static readonly Ldg Empty = new();

    public const short 
        BUYLDG = 1, 
        PURLDG = 2;

    public static readonly Map<short, string> Typs = new()
    {
        { 1, "商户" },
        { 2, "供源" },
        { 3, "中库" },
    };

    internal int orgid;

    internal DateTime dt;

    internal int typ;

    internal string name;

    internal short corgid;

    internal int trans;

    internal decimal amt;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(orgid), ref orgid);
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(typ), ref typ);
        s.Get(nameof(name), ref name);
        s.Get(nameof(corgid), ref corgid);
        s.Get(nameof(trans), ref trans);
        s.Get(nameof(amt), ref amt);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(orgid), orgid);
        s.Put(nameof(dt), dt);
        s.Put(nameof(typ), typ);
        s.Put(nameof(name), name);
        s.Put(nameof(corgid), corgid);
        s.Put(nameof(trans), trans);
        s.Put(nameof(amt), amt);
    }
}