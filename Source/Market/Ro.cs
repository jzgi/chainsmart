using SkyChain;
using Zhnt;

namespace Zhnt.Market
{
    /// <summary>
    /// A retail order item.
    /// </summary>
    public class Ro : _Doc, IKeyable<int>
    {
        public static readonly Ro Empty = new Ro();

        public const byte ID = 1, LATER = 4;

        public static readonly Map<short, string> Compls = new Map<short, string>
        {
            {0, "不需要申诉"},
            {1, "特殊原因无法安排日程，需要撤销整个订单"},
            {2, "对膳食品质有疑虑，需临时中断调养日程"},
            {3, "在已经做出配合的情况下，没有达到承诺的功效"},
            {4, "不满意服务质量或人员态度"},
        };

        public static readonly Map<short, string> Levels = new Map<short, string>
        {
            {1, "１０％"},
            {2, "２０％"},
            {3, "３０％"},
            {4, "４０％"},
            {10, "１００％"},
        };

        internal int id;
        internal short dietid;
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

        public override void Read(ISource s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            base.Read(s, proj);

            s.Get(nameof(dietid), ref dietid);
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

        public override void Write(ISink s, byte proj = 0x0f)
        {
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            base.Write(s, proj);

            s.Put(nameof(dietid), dietid);
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

        public bool IsRefundable => status == STATUS_ISSUED;

        public override bool IsShowable => status >= STATUS_ISSUED;

        public override bool IsWorkable => status >= STATUS_ISSUED && status < STATUS_CLOSED;
    }
}