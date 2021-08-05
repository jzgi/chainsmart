using SkyChain;

namespace Zhnt.Mart
{
    public struct OrderAgg : IData, IKeyable<int>
    {
        internal int seq;

        internal short targ; // itemid, ptid,

        internal short yet;

        internal short don;

        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(seq), ref seq);
            s.Get(nameof(targ), ref targ);
            s.Get(nameof(yet), ref yet);
            s.Get(nameof(don), ref don);
        }

        public void Write(ISink s, byte proj = 15)
        {
        }

        public int Key => seq;
    }
}