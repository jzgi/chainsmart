﻿using System;
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
        internal string ext;
        internal short store;
        internal short duration;
        internal bool agt; // agent only 
        internal string unit;
        internal string unitip;

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
            s.Get(nameof(ext), ref ext);
            s.Get(nameof(store), ref store);
            s.Get(nameof(duration), ref duration);
            s.Get(nameof(agt), ref agt);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitip), ref unitip);
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
            s.Put(nameof(ext), ext);
            s.Put(nameof(store), store);
            s.Put(nameof(duration), duration);
            s.Put(nameof(agt), agt);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitip), unitip);
        }

        public int Key => id;
    }
}