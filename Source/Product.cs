using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A bookable product from certain source. It can be upgraded to the group-book mode and then back.
    /// </summary>
    public class Product : Entity, IKeyable<int>
    {
        public static readonly Product Empty = new Product();

        public new static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_DISABLED, "禁用"},
            {STA_ENABLED, "正常"},
            {STA_TOP, "置顶"},
        };


        public static readonly Map<short, string> Stores = new Map<short, string>
        {
            {0, "常规"},
            {1, "冷藏"},
            {2, "冷冻"},
        };

        public static readonly Map<short, string> Durations = new Map<short, string>
        {
            {3, "三天"},
            {5, "五天"},
            {7, "一周"},
            {10, "十天"},
            {15, "半个月"},
            {30, "一个月"},
            {60, "两个月"},
            {90, "三个月"},
            {120, "四个月"},
            {180, "六个月"},
        };

        internal int id;

        internal int srcid;
        internal short store;
        internal short duration;
        internal bool agt; // agent only 
        internal string unit;
        internal string unitip;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(srcid), ref srcid);
            }
            s.Get(nameof(store), ref store);
            s.Get(nameof(duration), ref duration);
            s.Get(nameof(agt), ref agt);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
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
                s.Put(nameof(srcid), srcid);
            }
            s.Put(nameof(store), store);
            s.Put(nameof(duration), duration);
            s.Put(nameof(agt), agt);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
        }

        public int Key => id;

        public override string ToString() => name;
    }
}