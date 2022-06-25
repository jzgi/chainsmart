using System;
using CoChain;

namespace Revital
{
    /// <summary>
    /// A sellable ware model.
    /// </summary>
    /// <remarks>
    /// It can be upgraded to the group-purchase mode and then back. 
    /// </remarks>
    public class Ware : Info, IKeyable<int>
    {
        public static readonly Ware Empty = new Ware();

        public new static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_DISABLED, "停售"},
            {STA_ENABLED, "在售"},
            {STA_HOT, "冲量"},
        };


        public static readonly Map<short, string> Stores = new Map<short, string>
        {
            {0, "常规"},
            {1, "冷藏"},
            {2, "冷冻"},
        };

        public static readonly short[] Durations = {3, 5, 7, 10, 15, 20, 30, 45, 60, 90, 120};

        internal int id;

        internal int srcid;
        internal short itemid;
        internal string ext;
        internal short store;
        internal short duration;
        internal string unit;
        internal short unitx;

        // individual order relevant

        internal decimal price;
        internal short cap;
        internal short min;
        internal short max;
        internal short step;
        internal bool agt; // 

        // when changed to group-book mode

        internal DateTime expected;
        internal decimal off;
        internal short threshold;
        internal short addup;

        // extra
        internal int prvid;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((msk & BORN) == BORN)
            {
                s.Get(nameof(srcid), ref srcid);
            }
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(ext), ref ext);
            s.Get(nameof(store), ref store);
            s.Get(nameof(duration), ref duration);
            s.Get(nameof(agt), ref agt);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(cap), ref cap);

            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);

            s.Get(nameof(expected), ref expected);
            s.Get(nameof(off), ref off);
            s.Get(nameof(threshold), ref threshold);
            s.Get(nameof(addup), ref addup);

            if ((msk & EXTRA) == EXTRA)
            {
                s.Get(nameof(prvid), ref prvid);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if ((msk & BORN) == BORN)
            {
                s.Put(nameof(srcid), srcid);
            }
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(ext), ext);
            s.Put(nameof(store), store);
            s.Put(nameof(duration), duration);
            s.Put(nameof(agt), agt);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(cap), cap);

            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);

            s.Put(nameof(expected), expected);
            s.Put(nameof(off), off);
            s.Put(nameof(threshold), threshold);
            s.Put(nameof(addup), addup);
        }

        public int Key => id;
    }
}