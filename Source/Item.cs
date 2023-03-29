using ChainFx;

namespace ChainSmart
{
    public class Item : Entity, IKeyable<int>, IStockable
    {
        public static readonly Item Empty = new();

        public const short
            TYP_DEF = 1,
            TYP_REF = 2;

        public static readonly Map<short, string> Typs = new()
        {
            { TYP_DEF, "自建" },
            { TYP_REF, "引用" },
        };


        internal int id;
        internal int shpid;
        internal int lotid;
        internal short catid;
        internal string unit;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short minx;
        internal short stock;
        internal short avail;

        internal bool icon;
        internal bool pic;

        internal StockOp[] ops;

        public override void Read(ISource s, short msk = 255)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(lotid), ref lotid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(catid), ref catid);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(minx), ref minx);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(stock), ref stock);
                s.Get(nameof(avail), ref avail);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
            }

            if ((msk & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Get(nameof(ops), ref ops);
            }
        }

        public override void Write(ISink s, short msk = 255)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(shpid), shpid);
                if (lotid > 0) s.Put(nameof(lotid), lotid);
                else s.PutNull(nameof(lotid));
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(catid), catid);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(minx), minx);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(stock), stock);
                s.Put(nameof(avail), avail);
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
            }

            if ((msk & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Put(nameof(ops), ops);
            }
        }

        public int Key => id;

        public decimal RealPrice => price - off;

        public int StockX => stock / unitx;

        public int AvailX => avail / unitx;

        public StockOp[] Ops => ops;
    }
}