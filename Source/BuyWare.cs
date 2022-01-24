using SkyChain;

namespace Revital
{
    public class BuyWare : IData
    {
        public static readonly Buy Empty = new Buy();

        public long buyid;
        public int wareid;
        public string warename;
        public short itemid;
        public decimal price;
        public short qty;
        public decimal total;

        public void Read(ISource s, short proj = 0x0fff)
        {
            s.Get(nameof(buyid), ref buyid);
            s.Get(nameof(wareid), ref wareid);
            s.Get(nameof(warename), ref warename);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(qty), ref qty);
        }

        public void Write(ISink s, short proj = 0x0fff)
        {
            s.Put(nameof(buyid), buyid);
            s.Put(nameof(wareid), wareid);
            s.Put(nameof(warename), warename);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(qty), qty);
        }

        public long Key => buyid;
    }
}