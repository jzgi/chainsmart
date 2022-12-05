using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product by certain source.
    /// </summary>
    public class Item : Entity, IKeyable<int>
    {
        public static readonly Item Empty = new Item();

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
        internal string origin;
        internal JObj specs;
        internal bool icon;
        internal bool pic;
        internal bool m1;
        internal bool m2;
        internal bool m3;
        internal bool m4;

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
                s.Get(nameof(origin), ref origin);
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
                s.Put(nameof(origin), origin);
                s.Put(nameof(specs), specs);
            }
        }

        public int Key => id;

        public override string ToString() => name;
    }
}