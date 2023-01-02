using ChainFx;

namespace ChainMart
{
    public struct BuyDetail : IData, IKeyable<int>
    {
        public int wareid;

        public int itemid;

        public string name;

        public string unit;

        public decimal unitx;

        public decimal price;

        public decimal off;

        public short qty;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(wareid), ref wareid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(name), ref name);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(wareid), wareid);
            s.Put(nameof(name), name);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
        }

        public int Key => wareid;

        public decimal RealPrice => price - off;

        public decimal SubTotal => decimal.Round(RealPrice * unitx * qty, 2);


        internal void InitWithWare(Ware v, bool discount)
        {
            name = v.name;
            itemid = v.itemid;
            unit = v.unit;
            price = v.price;
            off = discount ? v.off : 0;
        }
    }
}