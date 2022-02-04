using SkyChain;

namespace Revital
{
    public class Way : _Info
    {
        public static readonly Way Empty = new Way();

        public const short
            TYP_NORM = 1,
            TYP_SPECIAL = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_NORM, "一般路径"},
            {TYP_SPECIAL, "特殊路径"},
        };

        internal int ctrid;

        internal int mrtid;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(mrtid), ref mrtid);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(mrtid), mrtid);
        }
    }
}