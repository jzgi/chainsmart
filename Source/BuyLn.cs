using Chainly;

namespace Revital
{
    public struct BuyLn : IData
    {
        public int prodid;
        public string prodname;
        public short itemid;
        public decimal price;
        public short qty;
        public short qtyre; // qty reducted

        public void Read(ISource s, short proj = 0xff)
        {
            s.Get(nameof(prodid), ref prodid);
            s.Get(nameof(prodname), ref prodname);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(qtyre), ref qtyre);
        }

        public void Write(ISink s, short proj = 0xff)
        {
            s.Put(nameof(prodid), prodid);
            s.Put(nameof(prodname), prodname);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(qty), qty);
            s.Put(nameof(qtyre), qtyre);
        }
    }
}