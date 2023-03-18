using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class Book : Entity, IKeyable<int>
    {
        public static readonly Book Empty = new();

        public const short
            TYP_SPOT = 1,
            TYP_FUTURE = 2;

        public static readonly Map<short, string> Typs = new()
        {
            { TYP_SPOT, "现货订单" },
            { TYP_FUTURE, "助农订单" },
        };

        public new static readonly Map<short, string> Statuses = new()
        {
            { STU_VOID, "撤单" },
            { STU_CREATED, "下单" },
            { STU_ADAPTED, "待发" },
            { STU_OKED, "发货" },
        };


        internal int id;

        internal int shpid; // shop
        internal string shpname;
        internal int mktid; // market
        internal int ctrid; // center
        internal int srcid; // source
        internal string srcname;

        internal int lotid;

        internal string unit;
        internal decimal unitx;
        internal decimal price;
        internal decimal off;
        internal decimal qty;
        internal decimal topay;
        internal decimal pay;
        internal decimal ret; // qty cut
        internal decimal refund; // pay refunded


        public Book()
        {
        }

        public Book(Lot lot, Org shp)
        {
            typ = lot.typ;
            name = lot.name;
            tip = lot.tip;

            shpid = shp.id;
            shpname = shp.Name;
            mktid = shp.MarketId;
            srcid = lot.srcid;
            srcname = lot.srcname;
            ctrid = shp.ctrid;

            lotid = lot.id;
            unit = lot.unit;
            unitx = lot.unitx;
            price = lot.price;
            off = lot.off;
        }

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
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(srcname), ref srcname);
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
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(srcname), srcname);
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

        public short QtyX => (short)(qty / unitx);

        public decimal RealPrice => price - off;

        public decimal Total => decimal.Round(RealPrice * qty, 2);

        public override string ToString() => shpname + "采购" + srcname + "产品" + name;

        public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
    }
}