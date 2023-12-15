using System;
using ChainFX;

namespace ChainSmart;

/// <summary>
/// A ledger entry record.
/// </summary>
public struct Ldg : IData
{
    public static readonly Ldg Empty = new();

    internal int orgid;

    internal DateTime dt;

    internal int acct;

    internal string name;

    internal int xorgid;

    internal int trans;

    internal int qty;

    internal decimal amt;

    public void Read(ISource s, short msk = 0xff)
    {
        s.Get(nameof(orgid), ref orgid);
        s.Get(nameof(dt), ref dt);
        s.Get(nameof(acct), ref acct);
        s.Get(nameof(name), ref name);
        s.Get(nameof(xorgid), ref xorgid);
        s.Get(nameof(trans), ref trans);
        s.Get(nameof(qty), ref qty);
        s.Get(nameof(amt), ref amt);
    }

    public void Write(ISink s, short msk = 0xff)
    {
        s.Put(nameof(orgid), orgid);
        s.Put(nameof(dt), dt);
        s.Put(nameof(acct), acct);
        s.Put(nameof(name), name);
        s.Put(nameof(xorgid), xorgid);
        s.Put(nameof(trans), trans);
        s.Put(nameof(qty), qty);
        s.Put(nameof(amt), amt);
    }
}