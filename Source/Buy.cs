using System;
using Chainly;

namespace Revital
{
    /// <summary>
    /// A buy to a particular product.
    /// </summary>
    public class Buy : Info, IKeyable<long>
    {
        public static readonly Buy Empty = new Buy();

        public const short
            STA_SRC_REFUND = 1,
            STA_PAID = 2,
            STA_ON_SRC = 3,
            STA_ON_CTR = 4,
            STA_ON_MRT = 5;


        public new static readonly Map<short, string> Statuses = new Map<short, string>
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

        internal int prodid;
        internal string prodname;
        internal short itemid;

        internal short mode;
        internal decimal price;
        internal decimal discount;
        internal short qty;
        internal decimal pay;


        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(bizid), ref bizid);
            s.Get(nameof(mrtid), ref mrtid);
            s.Get(nameof(ctrid), ref ctrid);
            s.Get(nameof(prvid), ref prvid);
            s.Get(nameof(srcid), ref srcid);

            s.Get(nameof(prodid), ref prodid);
            s.Get(nameof(prodname), ref prodname);
            s.Get(nameof(itemid), ref itemid);

            s.Get(nameof(mode), ref mode);
            s.Get(nameof(price), ref price);
            s.Get(nameof(discount), ref discount);
            s.Get(nameof(qty), ref qty);
            s.Get(nameof(pay), ref pay);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(bizid), bizid);
            s.Put(nameof(mrtid), mrtid);
            s.Put(nameof(ctrid), ctrid);
            s.Put(nameof(prvid), prvid);
            s.Put(nameof(srcid), srcid);

            s.Put(nameof(prodid), prodid);
            s.Put(nameof(prodname), prodname);
            s.Put(nameof(itemid), itemid);

            s.Put(nameof(mode), mode);
            s.Put(nameof(price), price);
            s.Put(nameof(discount), discount);
            s.Put(nameof(qty), qty);
            s.Put(nameof(pay), pay);
        }

        public long Key => id;

        public bool IsOver(DateTime now) => false;
    }
}