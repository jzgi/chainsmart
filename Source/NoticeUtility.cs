using System.Collections.Concurrent;
using System.Threading;
using ChainFx;

namespace ChainMart
{
    public class NoticeUtility
    {
        public const short
            NTC_BOOK_CREATED = 0,
            NTC_BOOK_ADAPTED = 1,
            NTC_BOOK_OKED = 2,
            NTC_BOOK_ABORTED = 3,
            NTC_BUY_CREATED = 4,
            NTC_BUY_ADAPTED = 5,
            NTC_BUY_OKED = 6,
            NTC_BUY_ABORTED = 7;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {NTC_BOOK_CREATED, "供应链新单"},
            {NTC_BOOK_ADAPTED, "供应链发货"},
            {NTC_BOOK_OKED, "供应链收货"},
            {NTC_BOOK_ABORTED, "供应链撤单"},
            {NTC_BUY_CREATED, "零售新单"},
            {NTC_BUY_ADAPTED, "零售发货"},
            {NTC_BUY_OKED, "零售收货"},
            {NTC_BUY_ABORTED, "零售撤单"},
        };


        static readonly ConcurrentDictionary<int, NoticeSlot> slots = new ConcurrentDictionary<int, NoticeSlot>();


        static readonly Thread pusher = new Thread(PushCycle);


        static async void PushCycle(object state)
        {
            while (true)
            {
                Thread.Sleep(1000 * 60 * 600); // 10 minutes

                foreach (var ety in slots)
                {
                    var slot = ety.Value;
                    if (slot.HasToPush)
                    {
                        // send

                        // WeixinUtility
                    }
                }
            }
        }

        public static void Add(int orgid, short typ, int count, decimal amount)
        {
            // bags.GetOrAdd(orgid, k =>
            // {
            //     
            // });
        }
    }
}