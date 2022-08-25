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
        internal int mrtid; // market
        internal int srcid; // source
        internal int prvid; // provision
        internal int ctrid; // center

        internal int productid;
        internal short lotid;

        internal string unit;
        internal string unitstd;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short qty;
        internal short cut; // qty cut
        internal decimal pay;
        internal decimal refund; // pay refunded

        internal string srcop;
        internal DateTime srcon;
        internal string ctrop;
        internal DateTime ctron;
        internal string shpop;
        internal DateTime shpon;


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
                s.Get(nameof(mrtid), ref mrtid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(prvid), ref prvid);
                s.Get(nameof(srcid), ref srcid);

                s.Get(nameof(productid), ref productid);
                s.Get(nameof(lotid), ref lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitstd), ref unitstd);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(qty), ref qty);
                s.Get(nameof(pay), ref pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(shpop), ref shpop);
                s.Get(nameof(shpon), ref shpon);
                s.Get(nameof(ctrop), ref ctrop);
                s.Get(nameof(ctron), ref ctron);
                s.Get(nameof(srcop), ref srcop);
                s.Get(nameof(srcon), ref srcon);

                s.Get(nameof(cut), ref cut);
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
                s.Put(nameof(mrtid), mrtid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(prvid), prvid);
                s.Put(nameof(srcid), srcid);

                s.Put(nameof(productid), productid);
                s.Put(nameof(lotid), lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitstd), unitstd);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(qty), qty);
                s.Put(nameof(pay), pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(shpop), shpop);
                s.Put(nameof(shpon), shpon);
                s.Put(nameof(ctrop), ctrop);
                s.Put(nameof(ctron), ctron);
                s.Put(nameof(srcop), srcop);
                s.Put(nameof(srcon), srcon);

                s.Put(nameof(cut), cut);
                s.Put(nameof(refund), refund);
            }
        }

        public int Key => id;

        public bool IsOver(DateTime now) => false;

        public short Status => status;
    }
}