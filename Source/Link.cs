using SkyChain;

namespace Revital
{
    public class Link : _Info
    {
        public static readonly Link Empty = new Link();

        public const short
            TYP_DOWN = 1,
            TYP_UP = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_DOWN, "下游"},
            {TYP_UP, "上游"},
        };

        internal int ctrid;
        internal int ptid;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(ptid), ref ptid);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(ptid), ptid);
        }
    }
}