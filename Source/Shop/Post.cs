using SkyChain;

namespace Revital.Shop
{
    public class Post : _Bean, IKeyable<int>
    {
        public static readonly Post Empty = new Post();


        internal int id;
        internal int bizid;
        internal int itemid;
        internal int supplyid;
        internal string unit;
        internal short unitx;
        internal short min;
        internal short max;
        internal short step;
        internal decimal price;
        internal decimal off;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(bizid), ref bizid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(supplyid), ref supplyid);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(bizid), bizid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(supplyid), supplyid);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);

            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
        }

        public int Key => id;
    }
}