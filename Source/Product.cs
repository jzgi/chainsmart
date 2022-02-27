using System;
using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data model for a particular product supply.
    /// </summary>
    public class Product : Info, IKeyable<int>
    {
        public static readonly Product Empty = new Product();

        public static readonly Map<short, string> Fillgs = new Map<short, string>
        {
            {0, "次日发货"},
            {1, "指定日期"},
        };

        public static readonly Map<short, string> Mrtgs = new Map<short, string>
        {
            {0, "不管市场价"},
            {1, "建议市场价"},
            {2, "市场价上限"},
            {3, "市场价下限"},
            {4, "统一市场价"},
        };

        internal int id;

        internal short fillg;
        internal DateTime fillon;
        internal short mrtg;
        internal decimal mrtprice;
        internal short rankg;

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

        public override void Read(ISource s, short mask = 0xff)
        {
            base.Read(s, mask);

            if ((mask & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((mask & BORN) == BORN)
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

            s.Get(nameof(fillg), ref fillg);
            s.Get(nameof(fillon), ref fillon);
            s.Get(nameof(mrtg), ref mrtg);
            s.Get(nameof(mrtprice), ref mrtprice);
            s.Get(nameof(rankg), ref rankg);
        }

        public override void Write(ISink s, short mask = 0xff)
        {
            base.Write(s, mask);

            if ((mask & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if ((mask & BORN) == BORN)
            {
                s.Put(nameof(orgid), orgid);
            }
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(ext), ext);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);
            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(cap), cap);

            s.Put(nameof(fillg), fillg);
            s.Put(nameof(fillon), fillon);
            s.Put(nameof(mrtg), mrtg);
            s.Put(nameof(mrtprice), mrtprice);
            s.Put(nameof(rankg), rankg);
        }

        public int Key => id;
    }
}