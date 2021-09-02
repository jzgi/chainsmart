using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// A downstream supply of a particular item.
    /// 
    public class Down : _Art
    {
        public static readonly Down Empty = new Down();

        short itemid;

        internal short min;
        internal short max;
        internal short least;
        internal short step;

        internal short srcid;

        internal bool img;

        internal bool testa;

        internal bool testb;
        
        decimal price;

        decimal discount;

        int[] agts;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(least), ref least);
            s.Get(nameof(step), ref step);
            s.Get(nameof(price), ref price);
            s.Get(nameof(discount), ref discount);
            s.Get(nameof(agts), ref agts);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(least), least);
            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(discount), discount);
            s.Put(nameof(agts), agts);
        }
    }
}