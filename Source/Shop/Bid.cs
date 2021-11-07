using SkyChain;
using Revital.Shop;

namespace Revital.Shop
{
    /// <summary>
    /// A retail order, for online and offline
    /// </summary>
    public class Bid : IData, IKeyable<int>
    {
        public static readonly Bid Empty = new Bid();

        public const byte ID = 1, LATER = 4;

        public const short
            STATUS_CREATED = 0,
            STATUS_SUBMITTED = 1, // before processing
            STATUS_ABORTED = 2,
            STATUS_CONFIRMED = 3, // ready for distr center op 
            STATUS_SHIPPED = 4, //  
            STATUS_CLOSED = 5; // after clearing

        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STATUS_CREATED, "草稿中"},
            {STATUS_SUBMITTED, "已提交"},
            {STATUS_ABORTED, "已撤销"},
            {STATUS_CONFIRMED, "已确认"},
            {STATUS_SHIPPED, "已发货"},
            {STATUS_CLOSED, "已关闭"},
        };
        internal int id;
        internal int bizid;
        internal string trackg;
        internal short level;
        internal short ptid;
        internal int uid;
        internal string uname;
        internal string utel;
        internal string uim;
        internal decimal price;
        internal decimal pay;
        internal decimal refund;
        internal decimal compl;

        public void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(bizid), ref bizid);
            s.Get(nameof(trackg), ref trackg);
            s.Get(nameof(level), ref level);
            s.Get(nameof(ptid), ref ptid);
            s.Get(nameof(uid), ref uid);
            s.Get(nameof(uname), ref uname);
            s.Get(nameof(utel), ref utel);
            s.Get(nameof(uim), ref uim);
            s.Get(nameof(price), ref price);
            if ((proj & LATER) == LATER)
            {
                s.Get(nameof(pay), ref pay);
                s.Get(nameof(refund), ref refund);
                s.Get(nameof(compl), ref compl);
            }
        }

        public void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(bizid), bizid);
            s.Put(nameof(trackg), trackg);
            s.Put(nameof(level), level);
            s.Put(nameof(ptid), ptid);
            s.Put(nameof(uid), uid);
            s.Put(nameof(uname), uname);
            s.Put(nameof(utel), utel);
            s.Put(nameof(uim), uim);
            s.Put(nameof(price), price);
            if ((proj & LATER) == LATER)
            {
                s.Put(nameof(pay), pay);
                s.Put(nameof(refund), refund);
                s.Put(nameof(compl), compl);
            }
        }

        public int Key => id;
    }
}