using CoChain;

namespace CoSupply
{
    public class Item : Entity, IKeyable<int>
    {
        public static readonly Item Empty = new Item();

        internal int id;
        internal int shpid;
        internal int productid;
        internal string unit;
        internal string unitip;
        internal decimal price;
        internal short min;
        internal short max;
        internal short step;

        public override void Read(ISource s, short msk = 255)
        {
            base.Read(s, msk);

            if ((msk & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & BORN) == BORN)
            {
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(productid), ref productid);
            }
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
            s.Get(nameof(price), ref price);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);
        }

        public override void Write(ISink s, short msk = 255)
        {
            base.Write(s, msk);

            if ((msk & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & BORN) == BORN)
            {
                s.Put(nameof(shpid), shpid);
                s.Put(nameof(productid), productid);
            }
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
            s.Put(nameof(price), price);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
        }

        public int Key => id;
    }
}