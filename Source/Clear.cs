using System;
using ChainFx;

namespace ChainMart
{
    public class Clear : Entity, IKeyable<int>
    {
        public static readonly Clear Empty = new Clear();

        public const short
            TYP_PLAT = 1,
            TYP_GATEWAY = 2,
            TYP_SRC = 3,
            TYP_ZON = 4,
            TYP_CTR = 5,
            TYP_SHP = 7,
            TYP_MKT = 8;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PLAT, "平台设施"},
            {TYP_GATEWAY, "支付网关"},
            {TYP_SRC, "产源销售"},
            {TYP_ZON, "供区盟主"},
            {TYP_CTR, "中库服务"},
            {TYP_SHP, "摊铺销售"},
            {TYP_MKT, "市场盟主"},
        };


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_CREATED, "结算"},
            {STU_ADAPTED, "确认"},
            {STU_OKED, "支付"},
        };

        internal int id;
        internal DateTime till;
        internal int orgid;
        internal int prtid;
        internal short trans;
        internal decimal amt;
        internal decimal rate;
        internal decimal topay;
        internal decimal pay;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(till), ref till);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(prtid), ref prtid);
            s.Get(nameof(trans), ref trans);
            s.Get(nameof(amt), ref amt);
            s.Get(nameof(rate), ref rate);
            s.Get(nameof(topay), ref topay);
            s.Get(nameof(pay), ref pay);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & MSK_EXTRA) == MSK_EXTRA)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(till), till);
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(prtid), prtid);
            s.Put(nameof(trans), trans);
            s.Put(nameof(amt), amt);
            s.Put(nameof(rate), rate);
            s.Put(nameof(topay), topay);
            s.Put(nameof(pay), pay);
        }

        public int Key => id;
    }
}