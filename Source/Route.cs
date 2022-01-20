using SkyChain;

namespace Revital
{
    public class Route : _Info
    {
        public static readonly Route Empty = new Route();

        public const short
            TYP_TOMRT = 1,
            TYP_TOCTR = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_TOMRT, "关联市场"},
            {TYP_TOCTR, "关联中转"},
        };

        internal int ctrid;
        internal int ptid; // mrtid or srcid

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(ptid), ref ptid);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(ptid), ptid);
        }
    }
}