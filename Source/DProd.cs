using SkyChain;

namespace Zhnt
{
    public class DProd : _Art
    {
        public static readonly DProd Empty = new DProd();

        short itemid;

        internal short min;
        internal short max;
        internal short least;
        internal short step;

        decimal price;

        decimal discount;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);
            
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(least), ref least);
            s.Get(nameof(step), ref step);

        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);
            
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(least), least);
            s.Put(nameof(step), step);

        }
    }
}