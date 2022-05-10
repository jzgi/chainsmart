using System;
using Chainly;

namespace Revital
{
    /// <summary>
    /// A product supply data model.
    /// </summary>
    /// <remarks>
    /// It can be upgraded to the group-book mode and then back. 
    /// </remarks>
    public class Product : Info, IKeyable<int>
    {
        public static readonly Product Empty = new Product();

        internal int id;

        internal int orgid;
        internal short itemid;
        internal string ext;
        internal string unit;
        internal short unitx;

        // individual order relevant

        internal int min;
        internal int max;
        internal int step;
        internal decimal price;
        internal bool authreq; // authorization required

        // when changed to group-book mode

        internal short mode; // 
        internal decimal discount;
        internal int threshold;
        internal DateTime deadline;

        internal int cap;
        internal int total;


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
            s.Get(nameof(authreq), ref authreq);

            s.Get(nameof(mode), ref mode);
            s.Get(nameof(discount), ref discount);
            s.Get(nameof(threshold), ref threshold);
            s.Get(nameof(deadline), ref deadline);

            s.Get(nameof(cap), ref cap);
            s.Get(nameof(total), ref total);
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
            }
            s.Put(nameof(itemid), itemid);
            s.Put(nameof(ext), ext);
            s.Put(nameof(unit), unit);
            s.Put(nameof(unitx), unitx);

            s.Put(nameof(min), min);
            s.Put(nameof(max), max);
            s.Put(nameof(step), step);
            s.Put(nameof(price), price);
            s.Put(nameof(authreq), authreq);

            s.Put(nameof(mode), mode);
            s.Put(nameof(discount), discount);
            s.Put(nameof(threshold), threshold);
            s.Put(nameof(deadline), deadline);

            s.Put(nameof(cap), cap);
            s.Put(nameof(total), total);
        }

        public int Key => id;
    }
}