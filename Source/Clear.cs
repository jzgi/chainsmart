using System;
using Chainly;

namespace Revital
{
    public class Clear : Info, IKeyable<int>
    {
        public static readonly Clear Empty = new Clear();

        public const short
            TYP_PURCH = 1,
            TYP_BUY = 2;


        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PURCH, "供应链"},
            {TYP_BUY, "零售"},
        };

        public const short
            STA_ = 0,
            STA_APPROVED = 2,
            STA_PAID = 3;


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_, "新结算"},
            {STA_APPROVED, "已确认"},
            {STA_PAID, "已支付"},
        };

        internal int id;
        internal DateTime dt;
        internal int orgid;
        internal int sprid;
        internal short orders;
        internal decimal total;
        internal decimal rate;
        internal decimal pay;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & EXTRA) == EXTRA)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(dt), ref dt);
            s.Get(nameof(orgid), ref orgid);
            s.Get(nameof(sprid), ref sprid);
            s.Get(nameof(orders), ref orders);
            s.Get(nameof(total), ref total);
            s.Get(nameof(rate), ref rate);
            s.Get(nameof(pay), ref pay);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & EXTRA) == EXTRA)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(dt), dt);
            s.Put(nameof(orgid), orgid);
            s.Put(nameof(sprid), sprid);
            s.Put(nameof(orders), orders);
            s.Put(nameof(total), total);
            s.Put(nameof(rate), rate);
            s.Put(nameof(pay), pay);
        }

        public int Key => id;
    }
}