using System;
using Chainly;

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

        public const short
            MOD_NULL = 0,
            MOD_AUTHREQ = 1,
            MOD_GROUP = 2;

        public static readonly Map<short, string> Mods = new Map<short, string>
        {
            {MOD_NULL, null},
            {MOD_AUTHREQ, "授权"},
            {MOD_GROUP, "团购"},
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
        internal bool toagt; // 

        // when changed to group-book mode

        internal DateTime starton;
        internal DateTime endon;
        internal decimal off;
        internal short threshold;
        internal short present;

        // extra
        internal int prvid;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((proj & BORN) == BORN)
            {
                s.Get(nameof(srcid), ref srcid);
            }
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(ext), ref ext);
            s.Get(nameof(store), ref store);
            s.Get(nameof(duration), ref duration);
            s.Get(nameof(toagt), ref toagt);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(price), ref price);
            s.Get(nameof(cap), ref cap);

            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);

            s.Get(nameof(starton), ref starton);
            s.Get(nameof(endon), ref endon);
            s.Get(nameof(off), ref off);
            s.Get(nameof(threshold), ref threshold);
            s.Get(nameof(present), ref present);

            if ((proj & EXTRA) == EXTRA)
            {
                s.Get(nameof(prvid), ref prvid);
            }
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if ((proj & BORN) == BORN)
            {
                s.Put(nameof(prvid), prvid);
                s.Put(nameof(srcid), srcid);
            }
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(ext), ext);
            s.Put(nameof(store), store);
            s.Put(nameof(duration), duration);
            s.Put(nameof(toagt), toagt);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(price), price);
            s.Put(nameof(cap), cap);

            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);

            s.Put(nameof(starton), starton);
            s.Put(nameof(endon), endon);
            s.Put(nameof(off), off);
            s.Put(nameof(threshold), threshold);
            s.Put(nameof(present), present);
        }

        public int Key => id;
    }
}