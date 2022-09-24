using ChainFx;

namespace ChainMart
{
    public class Stock : Entity, IKeyable<int>
    {
        public static readonly Stock Empty = new Stock();

        internal int id;
        internal int shpid;
        internal int itemid;
        internal string unit;
        internal string unitstd;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short min;
        internal short max;
        internal short step;

        public override void Read(ISource s, short msk = 255)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(itemid), ref itemid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitstd), ref unitstd);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(min), ref min);
                s.Get(nameof(max), ref max);
                s.Get(nameof(step), ref step);
            }
        }

        public override void Write(ISink s, short msk = 255)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(shpid), shpid);
                s.Put(nameof(itemid), itemid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitstd), unitstd);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(min), min);
                s.Put(nameof(max), max);
                s.Put(nameof(step), step);
            }
        }

        public int Key => id;
    }
}