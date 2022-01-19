using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for workflow document.
    /// </summary>
    public abstract class _Deal : _Info
    {
        public const short
            STA_CREATED = 1,
            STA_ABORTED = 2,
            STA_ADAPTED = 3,
            STA_HANDLED = 4;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {0, null},
            {STA_CREATED, "已提交"},
            {STA_ABORTED, "已撤销"},
            {STA_ADAPTED, "已接受"},
            {STA_HANDLED, "已处理"},
        };

        public const short
            ITEM = 0x0020,
            PAY = 0x0040,
            HANDLE = 0x0080;


        // item
        internal short itemid;
        internal int artid;
        internal int artname;
        internal decimal price;
        internal int qty;
        internal decimal amt;
        internal decimal fee;
        internal decimal pay;
        internal string cs;
        internal short state;
        internal Act[] trace;


        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ITEM) == ITEM)
            {
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(artid), ref artid);
                s.Get(nameof(artname), ref artname);
                s.Get(nameof(price), ref price);
                s.Get(nameof(qty), ref qty);
            }
            if ((proj & PAY) == PAY)
            {
                s.Get(nameof(amt), ref amt);
                s.Get(nameof(fee), ref fee);
                s.Get(nameof(pay), ref pay);
            }
            if ((proj & HANDLE) == HANDLE)
            {
                s.Get(nameof(cs), ref cs);
                s.Get(nameof(state), ref state);
                s.Get(nameof(trace), ref trace);
            }
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ITEM) == ITEM)
            {
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(artid), artid);
                s.Put(nameof(artname), artname);
                s.Put(nameof(price), price);
                s.Put(nameof(qty), qty);
            }
            if ((proj & PAY) == PAY)
            {
                s.Put(nameof(amt), amt);
                s.Put(nameof(fee), fee);
                s.Put(nameof(pay), pay);
            }
            if ((proj & HANDLE) == HANDLE)
            {
                s.Put(nameof(cs), cs);
                s.Put(nameof(state), state);
                s.Put(nameof(trace), trace);
            }
        }
    }
}