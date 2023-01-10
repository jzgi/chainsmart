using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class Book : Entity, IKeyable<int>
    {
        public static readonly Book Empty = new Book();


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_VOID, null},
            {STU_CREATED, "已下单"},
            {STU_ADAPTED, "已发货"},
            {STU_OKED, "已收货"},
        };


        internal int id;

        internal int shpid; // shop
        internal string shpname;
        internal int mktid; // market
        internal int ctrid; // center
        internal int srcid; // source
        internal string srcname;
        internal int zonid; // zone

        internal int itemid;
        internal int lotid;

        internal string unit;
        internal decimal unitx;
        internal decimal price;
        internal decimal off;
        internal short qty;
        internal decimal topay;
        internal decimal pay;
        internal decimal ret; // qty cut
        internal decimal refund; // pay refunded


        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(shpname), ref shpname);
                s.Get(nameof(mktid), ref mktid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(zonid), ref zonid);
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(srcname), ref srcname);
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(lotid), ref lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(qty), ref qty);
                s.Get(nameof(topay), ref topay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(pay), ref pay);
                s.Get(nameof(ret), ref ret);
                s.Get(nameof(refund), ref refund);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(shpid), shpid);
                s.Put(nameof(shpname), shpname);
                s.Put(nameof(mktid), mktid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(zonid), zonid);
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(srcname), srcname);
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(lotid), lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(qty), qty);
                s.Put(nameof(topay), topay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(pay), pay);
                s.Put(nameof(ret), ret);
                s.Put(nameof(refund), refund);
            }
        }

        public int Key => id;

        public decimal RealPrice => price - off;

        public decimal Total => decimal.Round(RealPrice * unitx * qty, 2);

        public override string ToString() => shpname + "采购" + srcname + "产品" + name;

        public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
    }
}