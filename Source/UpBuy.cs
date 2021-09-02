using SkyChain;

namespace Zhnt.Supply
{
    /// 
    /// A upstream line of trade.
    /// 
    public class UpBuy : _Doc, IKeyable<int>
    {
        public static readonly UpBuy Empty = new UpBuy();

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
        internal short itemid;
        internal decimal price;
        internal decimal discount;
        internal int qty;

        public override void Read(ISource s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(price), ref price);
            s.Get(nameof(discount), ref discount);
            s.Get(nameof(qty), ref qty);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(itemid), itemid);
            s.Put(nameof(price), price);
            s.Put(nameof(discount), discount);
            s.Put(nameof(qty), qty);
        }

        public int Key => id;
    }
}