using ChainFx;

namespace ChainMart
{
    public struct BuyLine : IData, IKeyable<int>
    {
        public int wareid;
        public int itemid;
        public string name;
        public string unit;
        public decimal price;
        public decimal off;
        public short qty;
        public short subtotal;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(wareid), ref wareid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(name), ref name);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(subtotal), ref subtotal);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(wareid), wareid);
            s.Put(nameof(name), name);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(unit), unit);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(subtotal), subtotal);
        }

        public int Key => wareid;

        internal void InitializeByWare(Ware v, bool discount)
        {
            name = v.name;
            itemid = v.itemid;
            unit = v.unit;
            price = v.price;
            off = discount ? v.off : 0;
        }
    }
}