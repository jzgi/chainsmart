using ChainFx;

namespace ChainSMart
{
    /// <summary>
    /// A product item from certain source.
    /// </summary>
    public class Item : Entity, IKeyable<int>
    {
        public static readonly Item Empty = new Item();

        public const short
            STA_VOID = 0,
            STA_PRE = 1,
            STA_FINE = 2,
            STA_TOP = 4;

        public static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_VOID, "其它"},
            {STA_PRE, "通货"},
            {STA_FINE, "进口"},
            {STA_TOP, "特品"},
        };

        public static readonly Map<short, string> Stores = new Map<short, string>
        {
            {0, "常规"},
            {1, "冷藏"},
            {2, "冷冻"},
        };

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_VOID, "无效"},
            {STU_CREATED, "创建"},
            {STU_ADAPTED, "调整"},
            {STU_OKED, "上线"},
        };


        internal int id;

        internal int srcid;
        internal string origin;
        internal short store;
        internal short duration;
        internal JObj specs;
        internal bool icon;
        internal bool pic;
        internal bool m1;
        internal bool m2;
        internal bool m3;
        internal bool m4;
        internal bool m5;
        internal bool m6;

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
                s.Get(nameof(origin), ref origin);
                s.Get(nameof(store), ref store);
                s.Get(nameof(duration), ref duration);
                s.Get(nameof(specs), ref specs);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(icon), ref icon);
                s.Get(nameof(pic), ref pic);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(m4), ref m4);
                s.Get(nameof(m5), ref m5);
                s.Get(nameof(m6), ref m6);
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
                s.Put(nameof(origin), origin);
                s.Put(nameof(store), store);
                s.Put(nameof(duration), duration);
                s.Put(nameof(specs), specs);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(icon), icon);
                s.Put(nameof(pic), pic);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
                s.Put(nameof(m5), m5);
                s.Put(nameof(m6), m6);
            }
        }

        public int Key => id;

        public override string ToString() => name;
    }
}