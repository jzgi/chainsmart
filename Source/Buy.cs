using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// An online retail buy order.
    /// </summary>
    public class Buy : Entity, IKeyable<long>
    {
        public static readonly Buy Empty = new();

        public const short
            TYP_PLAT = 1,
            TYP_CASH = 2,
            TYP_QRCODE = 3,
            TYP_BANK = 4;

        public static readonly Map<short, string> Typs = new()
        {
            { TYP_PLAT, "平台" },
            { TYP_CASH, "现金" },
            { TYP_QRCODE, "扫码" },
            { TYP_BANK, "银行" },
        };

        public new static readonly Map<short, string> Statuses = new()
        {
            { STU_VOID, null },
            { STU_CREATED, "下单" },
            { STU_ADAPTED, "发货" },
            { STU_OKED, "收货" },
            { STU_ABORTED, "撤单" },
        };


        internal long id;
        internal int shpid;
        internal int mktid;
        internal int uid;
        internal string uname;
        internal string utel;
        internal string uaddr;
        internal string uim;
        internal BuyLn[] lns; // detail lines
        internal decimal topay;
        internal decimal pay;
        internal decimal ret;
        internal decimal refund;

        public Buy()
        {
        }

        public Buy(User prin, Org shp, BuyLn[] arr)
        {
            typ = TYP_PLAT;
            name = shp.Name;
            shpid = shp.id;
            mktid = shp.MarketId;
            lns = arr;
            uid = prin.id;
            uname = prin.name;
            utel = prin.tel;
            uim = prin.im;
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
                s.Get(nameof(mktid), ref mktid);
                s.Get(nameof(uid), ref uid);
                s.Get(nameof(uname), ref uname);
                s.Get(nameof(utel), ref utel);
                s.Get(nameof(uaddr), ref uaddr);
                s.Get(nameof(uim), ref uim);
                s.Get(nameof(lns), ref lns);
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
                s.Put(nameof(mktid), mktid);

                if (uid > 0) s.Put(nameof(uid), uid);
                else s.PutNull(nameof(uid));

                s.Put(nameof(uname), uname);
                s.Put(nameof(utel), utel);
                s.Put(nameof(uaddr), uaddr);
                s.Put(nameof(uim), uim);
                s.Put(nameof(lns), lns);
                s.Put(nameof(topay), topay);
            }

            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(pay), pay);
                s.Put(nameof(ret), ret);
                s.Put(nameof(refund), refund);
            }
        }

        public void SetToPay()
        {
            var sum = 0.00M;
            if (lns != null)
            {
                for (int i = 0; i < lns.Length; i++)
                {
                    var dtl = lns[i];
                    sum += dtl.SubTotal;
                }
            }

            // set the topay field
            topay = sum;
        }

        public long Key => id;

        public override string ToString() => uname + "购买" + name + "商品";

        public static string GetOutTradeNo(int id, decimal topay) => (id + "-" + topay).Replace('.', '-');
    }
}