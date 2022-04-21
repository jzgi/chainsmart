﻿using Chainly;

namespace Revital
{
    /// <summary>
    /// The data model for a particular product supply.
    /// </summary>
    public class Product : Info, IKeyable<int>
    {
        public static readonly Product Empty = new Product();

        public static readonly Map<short, string> Mrtgs = new Map<short, string>
        {
            {0, "不管市场价"},
            {1, "建议市场价"},
            {2, "上限市场价"},
            {3, "下限市场价"},
            {4, "强制市场价"},
        };

        public static readonly Map<short, string> Bookgs = new Map<short, string>
        {
            {0, "无限制"},
            {1, "须市场授权"},
        };

        internal int id;

        internal int orgid;
        internal short itemid;
        internal string ext;
        internal string unit;
        internal short unitx;
        internal short min;
        internal short max;
        internal short step;
        internal decimal price;
        internal int cap;
        internal short mrtg;
        internal decimal mrtprice;
        internal bool auth;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            if ((proj & BORN) == BORN)
            {
                s.Get(nameof(orgid), ref orgid);
            }
            s.Get(nameof(itemid), ref itemid);
            s.Get(nameof(ext), ref ext);
            s.Get(nameof(unit), ref unit);
            s.Get(nameof(unitx), ref unitx);
            s.Get(nameof(min), ref min);
            s.Get(nameof(max), ref max);
            s.Get(nameof(step), ref step);
            s.Get(nameof(price), ref price);
            s.Get(nameof(cap), ref cap);
            s.Get(nameof(mrtg), ref mrtg);
            s.Get(nameof(mrtprice), ref mrtprice);
            s.Get(nameof(auth), ref auth);
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
                s.Put(nameof(orgid), orgid);
                s.Put(nameof(itemid), itemid);
            }
            s.Put(nameof(ext), ext);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(cap), cap);
            s.Put(nameof(mrtg), mrtg);
            s.Put(nameof(mrtprice), mrtprice);
            s.Put(nameof(auth), auth);
        }

        public int Key => id;
    }
}