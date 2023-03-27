using System;
using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// An entry record of ledger.
    /// </summary>
    public class Agg : IData
    {
        public static readonly Agg Empty = new();

        public const short BUYAGG = 1, BOOKAGG = 2;

        public static readonly Map<short, string> Typs = new()
        {
            { 1, "商户" },
            { 2, "供源" },
            { 3, "中库" },
        };

        internal int orgid;

        internal DateTime dt;

        internal short typ;

        internal short coid;

        internal int trans;

        internal decimal qty;

        internal decimal amt;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(typ), ref typ);
            s.Get(nameof(coid), ref coid);
            s.Get(nameof(trans), ref trans);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(amt), ref amt);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(dt), dt);
            s.Put(nameof(typ), typ);
            s.Put(nameof(coid), coid);
            s.Put(nameof(trans), trans);
            s.Put(nameof(qty), qty);
            s.Put(nameof(amt), amt);
        }
    }
}