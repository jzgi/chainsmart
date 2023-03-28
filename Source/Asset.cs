using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// An asset such as land or truck.
    /// </summary>
    public class Asset : Entity, IKeyable<int>
    {
        public static readonly Asset Empty = new();

        public static readonly Map<short, string> Typs = new()
        {
            { 1, "地块" },
            { 2, "养殖" },
            { 4, "屠宰" },
            { 8, "进口" },
        };

        public static readonly Map<short, string> Ranks = new()
        {
            { 0, "通货" },
            { 1, "零添" },
            { 2, "特品" },
            { 4, "进口" },
        };

        internal short id;

        internal int orgid;
        internal short rank;
        internal int cap;
        internal string cern; // carbon emission reduction number
        internal double factor;
        internal double x;
        internal double y;
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
                s.Get(nameof(orgid), ref orgid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(cap), ref cap);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
                s.Get(nameof(rank), ref rank);
                s.Get(nameof(specs), ref specs);
                s.Get(nameof(cern), ref cern);
                s.Get(nameof(factor), ref factor);
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
                s.Put(nameof(orgid), orgid);
            }

            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(cap), cap);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
                s.Put(nameof(rank), rank);
                s.Put(nameof(cern), cern);
                s.Put(nameof(factor), factor);
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
            }
        }

        public int Key => id;

        public override string ToString() => name;
    }
}