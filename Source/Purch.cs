using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// A upstream line of purchase.
    /// 
    public class Purch : _Doc, IKeyable<int>
    {
        public static readonly Purch Empty = new Purch();

        public const byte ID = 1, LATER = 2;

        public const short
            TYP_PRODUCT = 1,
            TYP_SERVICE = 2,
            TYP_EVENT = 3;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PRODUCT, "产品拼团"},
            {TYP_SERVICE, "服务拼团"},
            {TYP_EVENT, "社工活动"},
        };


        internal int id;

        // doc number
        internal int no;
        internal short productid;
        internal short itemid;
        internal decimal price;
        internal decimal off;
        internal int qty;
        internal decimal pay;
        internal decimal refund;

        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            base.Read(s, proj);

            s.Get(nameof(no), ref no);
            s.Get(nameof(productid), ref productid);
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(off), ref off);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(pay), ref pay);
            s.Get(nameof(refund), ref refund);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            base.Write(s, proj);

            s.Put(nameof(no), no);
            s.Put(nameof(productid), productid);
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(off), off);
            s.Put(nameof(qty), qty);
            s.Put(nameof(pay), pay);
            s.Put(nameof(refund), refund);
        }

        public int Key => id;
    }
}