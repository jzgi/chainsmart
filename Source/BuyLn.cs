using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// A detail line of buy.
    /// </summary>
    public class BuyLn : IData, IKeyable<int>
    {
        public int itemid;

        public int lotid;

        public string name;

        public string unit; // basic unit

        public decimal unitx; // number of units per pack

        public decimal price;

        public decimal off;

        public decimal qty;

        public BuyLn()
        {
        }

        public BuyLn(int itemid, decimal qty)
        {
            this.itemid = itemid;
            this.qty = qty;
        }

        public BuyLn(int itemid, string[] comp)
        {
            this.itemid = itemid;

            lotid = int.Parse(comp[0]);
            name = comp[1];
            unit = comp[2];
            unitx = decimal.Parse(comp[3]);
            price = decimal.Parse(comp[4]);
            qty = decimal.Parse(comp[5]);
        }

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(lotid), ref lotid);
            s.Get(nameof(name), ref name);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
        }

        public void Write(ISink s, short msk = 0xff)
        {
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(lotid), lotid);
            s.Put(nameof(name), name);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
        }

        public int Key => itemid;

        public decimal RealPrice => price - off;

        public decimal SubTotal => decimal.Round(RealPrice * qty, 2);

        public short QtyX => (short)(qty / unitx);

        internal void Init(Item m, bool discount)
        {
            name = m.name;
            lotid = m.lotid;
            unit = m.unit;
            unitx = m.unitx;
            price = m.price;

            if (discount)
            {
                off = m.off;
            }
        }
    }
}