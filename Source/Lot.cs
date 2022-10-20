using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product lot for booking.
    /// </summary>
    public class Lot : Entity, IKeyable<int>
    {
        public static readonly Lot Empty = new Lot();

        public const short
            STA_CREATED = 0,
            STA_SUBMITTED = 1,
            STU_OKED = 3;

        public static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_CREATED, "新创建"},
            {STA_SUBMITTED, "已提交"},
            {STU_OKED, "已验证"},
        };

        internal int id;
        internal int itemid;
        internal int srcid;
        internal int ctrid;
        internal bool ctring; // required centering

        // individual order relevant
        internal decimal price;
        internal decimal off;
        internal int cap;
        internal int remain;
        internal short min;
        internal short max;
        internal short step;

        internal int nstart;
        internal int nend;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(ctring), ref ctring);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(min), ref min);
                s.Get(nameof(max), ref max);
                s.Get(nameof(step), ref step);
                s.Get(nameof(cap), ref cap);
                s.Get(nameof(remain), ref remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(adapted), ref adapted);
                s.Get(nameof(adapter), ref adapter);
                s.Get(nameof(nstart), ref nstart);
                s.Get(nameof(nend), ref nend);
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
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(ctring), ctring);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(min), min);
                s.Put(nameof(max), max);
                s.Put(nameof(step), step);
                s.Put(nameof(cap), cap);
                s.Put(nameof(remain), remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(adapted), adapted);
                s.Put(nameof(adapter), adapter);
                s.Put(nameof(nstart), nstart);
                s.Put(nameof(nend), nend);
            }
        }

        public int Key => id;

        public override string ToString() => name;
    }
}