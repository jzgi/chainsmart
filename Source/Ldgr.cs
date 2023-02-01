using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// An entry record of ledger.
    /// </summary>
    public class Ldgr : IData
    {
        public static readonly Ldgr Empty = new Ldgr();

        public const short
            TYP_MRT = 1,
            TYP_PRV = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_MRT, "市场"},
            {TYP_PRV, "供给"},
        };

        internal int orgid;

        internal int acct;

        internal DateTime dt;

        internal short prtid;

        internal int trans;

        internal decimal qty;

        internal decimal amt;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(acct), ref acct);
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(prtid), ref prtid);
            s.Get(nameof(trans), ref trans);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(amt), ref amt);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(acct), acct);
            s.Put(nameof(dt), dt);
            s.Put(nameof(prtid), prtid);
            s.Put(nameof(trans), trans);
            s.Put(nameof(qty), qty);
            s.Put(nameof(amt), amt);
        }
    }
}