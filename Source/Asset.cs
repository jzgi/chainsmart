using ChainFx;

namespace ChainSmart
{
    /// <summary>
    /// An asset such as land or truck.
    /// </summary>
    public class Asset : Entity, IKeyable<int>
    {
        public static readonly Asset Empty = new Asset();

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {1, "地块"},
            {2, "养殖场"},
            {7, "车辆"},
        };

        public static readonly Map<short, string> States = new Map<short, string>
        {
            {0, null},
            {1, "达标"},
            {2, "健康"},
            {4, "生态"},
        };

        public const short
            STA_VOID = 0,
            STA_PRE = 1,
            STA_FINE = 2,
            STA_TOP = 4;

        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_VOID, "无效"},
            {STU_CREATED, "创建"},
            {STU_ADAPTED, "调整"},
            {STU_OKED, "上线"},
        };


        internal short id;

        internal int orgid;
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
                s.Get(nameof(cern), ref cern);
                s.Get(nameof(factor), ref factor);
                s.Get(nameof(x), ref x);
                s.Get(nameof(y), ref y);
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
                s.Put(nameof(orgid), orgid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(cap), cap);
                s.Put(nameof(cern), cern);
                s.Put(nameof(factor), factor);
                s.Put(nameof(x), x);
                s.Put(nameof(y), y);
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