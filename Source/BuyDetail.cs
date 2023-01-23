using ChainFx;

namespace ChainMart
{
    public class BuyDetail : IData, IKeyable<int>
    {
        public int wareid;

        public int itemid;

        public string name;

        public string unit; // basic unit

        public decimal unitx; // number of units per pack

        public decimal price;

        public decimal off;

        public decimal qty;

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

        public decimal SubTotal => decimal.Round(RealPrice * qty, 2);

        public short QtyX => (short) (qty / unitx);

        internal void InitByWare(Ware w, bool offed)
        {
            name = w.name;
            itemid = w.itemid;
            unit = w.unit;
            unitx = w.unitx;
            price = w.price;
            if (offed)
            {
                off = w.off;
            }
        }
    }
}