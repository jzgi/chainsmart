using SkyChain;

namespace Zhnt.Mart
{
    public struct Ingr : IData, IKeyable<int>
    {
        internal short id;
        internal short qty;

        public void Read(ISource s, byte proj = 15)
        {
            s.Get(nameof(id), ref id);
            s.Get(nameof(qty), ref qty);
        }

        public void Write(ISink s, byte proj = 15)
        {
            s.Put(nameof(id), id);
            s.Put(nameof(qty), qty);
        }

        public int Key => id;
    }
}