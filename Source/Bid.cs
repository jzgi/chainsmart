using SkyChain;

namespace Revital
{
    /// 
    /// A bid to source content.
    /// 
    public class Bid : _Doc, IKeyable<int>
    {
        public static readonly Bid Empty = new Bid();


        internal int id;
        internal short itemid;
        internal short planid;
        internal short yieldid;
        internal decimal price;
        internal decimal off;
        internal int qty;
        internal decimal pay;
        internal decimal refund;
        internal int codend;
        internal short codes;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(planid), ref planid);
            s.Get(nameof(yieldid), ref yieldid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(refund), ref refund);
            s.Get(nameof(codend), ref codend);
            s.Get(nameof(codes), ref codes);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(planid), planid);
            s.Put(nameof(yieldid), yieldid);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(pay), pay);
            s.Put(nameof(refund), refund);
            s.Put(nameof(codend), codend);
            s.Put(nameof(codes), codes);
        }

        public int Key => id;
    }
}