using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class Book : Entity, IKeyable<int>
    {
        public static readonly Book Empty = new Book();

        // states
        public const short
            STU_SHP_ = 0, // shop
            STU_SHP_REF = 1, // refund
            STU_SRC_GOT = 3, // paid
            STU_SRC_RET = 4,
            STU_SRC_SNT = 5,
            STU_CTR_RCVD = 6, // received
            STU_CTR_RET = 7, // returned
            STU_CTR_SNT = 8, // sent
            STU_SHP_RCV = 9;


        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {0, null},
            {STU_SHP_REF, "产源退款"},
            {STU_SRC_GOT, "商户付款"},
            {STU_SRC_RET, "中库退返"},
            {STU_SRC_SNT, "产源发货"},
            {STU_CTR_RCVD, "暂入中库"},
            {STU_CTR_RET, "商户拒收"},
            {STU_CTR_SNT, "中库运出"},
            {STU_SHP_RCV, "商户确收"},
        };


        internal int id;

        internal int shpid; // shop
        internal string shpop;
        internal DateTime shpon;
        internal int prvid; // provision
        internal string prvop;
        internal DateTime prvon;
        internal int srcid; // source
        internal string srcop;
        internal DateTime srcon;
        internal int ctrid; // center
        internal string ctrop;
        internal DateTime ctron;
        internal int mrtid; // market
        internal string mrtop;
        internal DateTime mrton;

        internal int productid;
        internal short distribid;

        internal DateTime shipat;
        internal string unit;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short qty;
        internal short qtyre; // qty reduced
        internal decimal pay;
        internal decimal payre; // pay refunded

        internal short status;


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
                s.Get(nameof(shpop), ref shpop);
                s.Get(nameof(shpon), ref shpon);
                s.Get(nameof(mrtid), ref mrtid);
                s.Get(nameof(mrtop), ref mrtop);
                s.Get(nameof(mrton), ref mrton);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(ctrop), ref ctrop);
                s.Get(nameof(ctron), ref ctron);
                s.Get(nameof(prvid), ref prvid);
                s.Get(nameof(prvop), ref prvop);
                s.Get(nameof(prvon), ref prvon);
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(srcop), ref srcop);
                s.Get(nameof(srcon), ref srcon);

                s.Get(nameof(productid), ref productid);
                s.Get(nameof(distribid), ref distribid);

                s.Get(nameof(shipat), ref shipat);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(qty), ref qty);
                s.Get(nameof(pay), ref pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(qtyre), ref qtyre);
                s.Get(nameof(payre), ref payre);
                s.Get(nameof(status), ref status);
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
                s.Put(nameof(shpop), shpop);
                s.Put(nameof(shpon), shpon);
                s.Put(nameof(mrtid), mrtid);
                s.Put(nameof(mrtop), mrtop);
                s.Put(nameof(mrton), mrton);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(ctrop), ctrop);
                s.Put(nameof(ctron), ctron);
                s.Put(nameof(prvid), prvid);
                s.Put(nameof(prvop), prvop);
                s.Put(nameof(prvon), prvon);
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(srcop), srcop);
                s.Put(nameof(srcon), srcon);


                s.Put(nameof(productid), productid);
                s.Put(nameof(distribid), distribid);

                s.Put(nameof(shipat), shipat);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(qty), qty);
                s.Put(nameof(pay), pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(qtyre), qtyre);
                s.Put(nameof(payre), payre);
                s.Put(nameof(status), status);
            }
        }

        public int Key => id;

        public bool IsOver(DateTime now) => false;

        public short Status => status;
    }
}