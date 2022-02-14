using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// A reportive record of daily transaction for goods.
    /// </summary>
    public class Daily : _Info
    {
        public static readonly Daily Empty = new Daily();

        public const short
            TYP_MRT = 1,
            TYP_PRV = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_MRT, "市场"},
            {TYP_PRV, "供给"},
        };

        internal int orgid;

        internal DateTime dt;

        internal short itemid;

        internal int count;

        internal decimal amt;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(count), ref count);
            s.Get(nameof(amt), ref amt);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            s.Put(nameof(orgid), orgid);
            s.Put(nameof(dt), dt);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(count), count);
            s.Put(nameof(amt), amt);
        }
    }
}