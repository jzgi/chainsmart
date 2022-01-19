using System;
using SkyChain;

namespace Revital
{
    public class Clear : _Info
    {
        public static readonly Clear Empty = new Clear();

        public const short
            TYP_RETAIL = 1,
            TYP_SUPPLY = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_RETAIL, "零售"},
            {TYP_SUPPLY, "供应链"},
        };

        internal int orgid;
        internal DateTime dt;
        internal int sprid;
        internal short count;
        internal decimal amt;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(count), ref count);
            s.Get(nameof(amt), ref amt);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            s.Put(nameof(orgid), orgid);
            s.Put(nameof(dt), dt);
            s.Put(nameof(sprid), sprid);
            s.Put(nameof(count), count);
            s.Put(nameof(amt), amt);
        }
    }
}