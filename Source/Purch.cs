using System;
using CoChain;

namespace Revital
{
    /// <summary>
    /// A purchase process to product.
    /// </summary>
    public class Purch : Info, IKeyable<int>, IFlowable
    {
        public static readonly Purch Empty = new Purch();

        
        // states
        public const short
            STU_BIZ_ = 0,
            STU_BIZ_REF = 1, // refund
            STU_SRC_GOT = 3, // paid
            STU_SRC_RET = 4,
            STU_SRC_SNT = 5,
            STU_CTR_RCVD = 6, // received
            STU_CTR_RET = 7, // returned
            STU_CTR_SNT = 8, // sent
            STU_BIZ_RCV = 9;


        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {0, null},
            {STU_BIZ_REF, "产源退款"},
            {STU_SRC_GOT, "商户付款"},
            {STU_SRC_RET, "中库退返"},
            {STU_SRC_SNT, "产源发货"},
            {STU_CTR_RCVD, "暂入中库"},
            {STU_CTR_RET, "商户拒收"},
            {STU_CTR_SNT, "中库运出"},
            {STU_BIZ_RCV, "商户确收"},
        };


        internal int id;

        internal int bizid;
        internal int prvid;
        internal int srcid;
        internal int ctrid;
        internal int mrtid;

        internal int wareid;
        internal short itemid;

        internal DateTime targeted;
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
        internal short status;


        #region FOR-BUY

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

                s.Get(nameof(targeted), ref targeted);
                s.Get(nameof(wareid), ref wareid);
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
                s.Get(nameof(status), ref status);
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

                s.Put(nameof(targeted), targeted);
                s.Put(nameof(wareid), wareid);
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
                s.Put(nameof(status), status);
            }
        }

        public int Key => id;

        public bool IsOver(DateTime now) => false;

        public short Status => status;
    }
}