using Chainly;

namespace Revital
{
    public class OrderLn : IData
    {
        public static readonly Order Empty = new Order();

        public long orderid;
        public int prodid;
        public string prodname;
        public short itemid;
        public decimal price;
        public short qty;

        public void Read(ISource s, short proj = 0xff)
        {
            s.Get(nameof(orderid), ref orderid);
            s.Get(nameof(prodid), ref prodid);
            s.Get(nameof(prodname), ref prodname);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(qty), ref qty);
        }

        public void Write(ISink s, short proj = 0xff)
        {
            s.Put(nameof(orderid), orderid);
            s.Put(nameof(prodid), prodid);
            s.Put(nameof(prodname), prodname);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(qty), qty);
        }

        public long Key => orderid;
    }
}