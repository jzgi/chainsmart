using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for a merchandise article.
    /// </summary>
    public abstract class _Article : _Info
    {
        public const short 
            ORG = 0x0020, 
            ITEM = 0x0040;


        // the specialized extensible discriminator
        internal int orgid;
        internal short itemid;
        internal short cat;
        internal string ext;
        internal string unit;
        internal short unitx;
        internal short min;
        internal short max;
        internal short step;
        internal decimal price;
        internal int cap;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ORG) == ORG)
            {
                s.Get(nameof(orgid), ref orgid);
            }
            if ((proj & ITEM) == ITEM)
            {
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(cat), ref cat);
                s.Get(nameof(ext), ref ext);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(min), ref min);
                s.Get(nameof(max), ref max);
                s.Get(nameof(step), ref step);
                s.Get(nameof(price), ref price);
                s.Get(nameof(cap), ref cap);
            }
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ORG) == ORG)
            {
                s.Put(nameof(orgid), orgid);
            }
            if ((proj & ITEM) == ITEM)
            {
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(cat), cat);
                s.Put(nameof(ext), ext);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(min), min);
                s.Put(nameof(max), max);
                s.Put(nameof(step), step);
                s.Put(nameof(price), price);
                s.Put(nameof(cap), cap);
            }
        }
    }
}