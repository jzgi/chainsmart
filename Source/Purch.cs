using System;
using Chainly;

namespace Revital
{
    /// <summary>
    /// A buy to a particular product.
    /// </summary>
    public class Purch : Info, IKeyable<long>
    {
        public static readonly Purch Empty = new Purch();

        // states
        public const short
            STA_SRC_REFUND = 1,
            STA_PAID = 2,
            STA_ON_SRC = 3,
            STA_ON_CTR = 4,
            STA_ON_MRT = 5;


        public static readonly Map<short, string> States = new Map<short, string>
        {
            {0, null},
            {STA_SRC_REFUND, "已退款"},
            {STA_PAID, "已付款"},
            {STA_ON_SRC, "备发货"},
            {STA_ON_CTR, "到中库"},
            {STA_ON_MRT, "到市场"},
        };


        internal long id;

        internal int bizid;
        internal int prvid;
        internal int srcid;
        internal int ctrid;
        internal int mrtid;

        internal short mode;
        internal int prodid;
        internal string prodname;
        internal short itemid;

        internal string unit;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short qty;
        internal decimal pay;
        internal short qtyre; // qty reduced
        internal decimal payre; // pay refunded

        // workflow
        internal PurchOp[] ops;
        internal short state;


        #region FOR-BUY

        internal string bunit;
        internal short bunitx;
        internal decimal bprice;
        internal short bmin;
        internal short bmax;
        internal short bstep;

        #endregion

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((proj & BORN) == BORN)
            {
                s.Get(nameof(bizid), ref bizid);
                s.Get(nameof(mrtid), ref mrtid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(prvid), ref prvid);
                s.Get(nameof(srcid), ref srcid);

                s.Get(nameof(mode), ref mode);
                s.Get(nameof(prodid), ref prodid);
                s.Get(nameof(prodname), ref prodname);
                s.Get(nameof(itemid), ref itemid);

                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(qty), ref qty);
                s.Get(nameof(pay), ref pay);
            }
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(qtyre), ref qtyre);
                s.Get(nameof(payre), ref payre);
                s.Get(nameof(ops), ref ops);
                s.Get(nameof(state), ref state);
            }
            if ((proj & EXTRA) == EXTRA)
            {
                s.Get(nameof(bunit), ref bunit);
                s.Get(nameof(bunitx), ref bunitx);
                s.Get(nameof(bprice), ref bprice);
                s.Get(nameof(bmin), ref bmin);
                s.Get(nameof(bmax), ref bmax);
                s.Get(nameof(bstep), ref bstep);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((proj & BORN) == BORN)
            {
                s.Put(nameof(bizid), bizid);
                s.Put(nameof(mrtid), mrtid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(prvid), prvid);
                s.Put(nameof(srcid), srcid);

                s.Put(nameof(mode), mode);
                s.Put(nameof(prodid), prodid);
                s.Put(nameof(prodname), prodname);
                s.Put(nameof(itemid), itemid);

                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(qty), qty);
                s.Put(nameof(pay), pay);
            }
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(qtyre), qtyre);
                s.Put(nameof(payre), payre);
                s.Put(nameof(ops), ops);
                s.Put(nameof(state), state);
            }
            if ((proj & EXTRA) == EXTRA)
            {
                s.Put(nameof(bunit), bunit);
                s.Put(nameof(bunitx), bunitx);
                s.Put(nameof(bprice), bprice);
                s.Put(nameof(bmin), bmin);
                s.Put(nameof(bmax), bmax);
                s.Put(nameof(bstep), bstep);
            }
        }

        public long Key => id;

        public bool IsOver(DateTime now) => false;
    }
}