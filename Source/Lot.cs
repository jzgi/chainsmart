using System;
using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// A product lot for booking.
    /// </summary>
    public class Lot : Entity, IKeyable<int>, IStockable
    {
        public static readonly Lot Empty = new();

        public const short
            TYP_SPOT = 1,
            TYP_LIFT = 2;

        public static readonly Map<short, string> Typs = new()
        {
            { TYP_SPOT, "现货" },
            { TYP_LIFT, "助农" },
        };

        internal int id;
        internal int srcid;
        internal string srcname;

        internal int[] targs; // (optional) targeted centers or markets
        internal short catid;
        internal DateTime dated;

        // individual order relevant
        internal int assetid;

        internal string unit;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short min;
        internal int cap;
        internal int stock;
        internal int avail;

        internal int nstart;
        internal int nend;
        internal bool icon;
        internal bool pic;
        internal bool m1;
        internal bool m2;
        internal bool m3;
        internal bool m4;

        internal StockOp[] ops;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(srcname), ref srcname);
                s.Get(nameof(assetid), ref assetid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(targs), ref targs);
                s.Get(nameof(catid), ref catid);
                s.Get(nameof(dated), ref dated);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(min), ref min);
                s.Get(nameof(cap), ref cap);
                s.Get(nameof(stock), ref stock);
                s.Get(nameof(avail), ref avail);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(nstart), ref nstart);
                s.Get(nameof(nend), ref nend);
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(m4), ref m4);
            }

            if ((msk & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Get(nameof(ops), ref ops);
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
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(srcname), srcname);
                if (assetid > 0) s.Put(nameof(assetid), assetid);
                else s.PutNull(nameof(assetid));
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(targs), targs);
                s.Put(nameof(catid), catid);
                s.Put(nameof(dated), dated);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(min), min);
                s.Put(nameof(cap), cap);
                s.Put(nameof(stock), stock);
                s.Put(nameof(avail), avail);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(nstart), nstart);
                s.Put(nameof(nend), nend);
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
            }

            if ((msk & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Put(nameof(ops), ops);
            }
        }

        public int Key => id;

        public decimal RealPrice => price - off;

        public bool IsAvailableFor(int mktid)
        {
            return targs == null || targs.Contains(mktid);
        }

        public int AvailX => avail / unitx;

        public StockOp[] Ops => ops;

        public bool IsSpot => typ == 1;

        public bool IsFuture => typ == 2;

        public int MaxForSingleBook => Math.Min(avail, 200);

        public override string ToString() => name;
    }
}