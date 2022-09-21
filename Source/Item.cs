using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product by certain source.
    /// </summary>
    public class Item : Entity, IKeyable<int>
    {
        public static readonly Item Empty = new Item();

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_VOID, "禁用"},
            {STA_NORMAL, "正常"},
            {STA_TOP, "置顶"},
        };

        public static readonly Map<short, string> Stores = new Map<short, string>
        {
            {0, "常规"},
            {1, "冷藏"},
            {2, "冷冻"},
        };

        internal int id;

        internal int srcid;
        internal short store;
        internal short duration;
        internal bool agt; // agent only 
        internal string unit;
        internal string unitstd;
        internal short unitx;

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
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(store), ref store);
                s.Get(nameof(duration), ref duration);
                s.Get(nameof(agt), ref agt);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitstd), ref unitstd);
                s.Get(nameof(unitx), ref unitx);
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
                s.Put(nameof(srcid), srcid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(store), store);
                s.Put(nameof(duration), duration);
                s.Put(nameof(agt), agt);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitstd), unitstd);
                s.Put(nameof(unitx), unitx);
            }
        }

        public int Key => id;

        public override string ToString() => name;
    }
}