using CoChain;

namespace Revital
{
    public class Stock : Info, IKeyable<int>
    {
        public static readonly Stock Empty = new Stock();

        internal int id;
        internal int bizid;
        internal int wareid;
        internal string unit;
        internal short unitx;
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
                s.Get(nameof(bizid), ref bizid);
                s.Get(nameof(wareid), ref wareid);
            }
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
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
                s.Put(nameof(bizid), bizid);
                s.Put(nameof(wareid), wareid);
            }
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
        }

        public int Key => id;
    }
}