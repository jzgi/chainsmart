using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class BookAgg : IData, IKeyable<int>
    {
        public static readonly BookAgg Empty = new BookAgg();

        // states
        public const short
            STA_CREATED = 0, // shop
            STA_PAID = 1, // paid
            STA_CANCELLED = 2, // 
            STA_CONFIRMED = 3, // confirmed and reveal to the center
            STA_REJECTED = 4, // center rejected
            STA_DELIVERED = 5, // delivered by the center
            STA_DENIED = 6, // shop returned
            STA_RECEIVED = 7; // shop received


        public static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_CREATED, null},
            {STA_PAID, "已付款"},
            {STA_CANCELLED, "已撤单"},
            {STA_CONFIRMED, "已确单"},
            {STA_REJECTED, "已回拒"},
            {STA_DELIVERED, "已发货"},
            {STA_DENIED, "已拒收"},
            {STA_RECEIVED, "已收货"},
        };


        internal int shpid; // shop
        internal int shpname;
        internal int count;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(shpid), ref shpid);
            s.Get(nameof(shpname), ref shpname);
            s.Get(nameof(count), ref count);
        }

        public void Write(ISink s, short msk = 0xff)
        {
        }

        public int Key => shpid;
    }
}