using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for post entry.
    /// </summary>
    public class Piece : Info, IKeyable<int>
    {
        public static readonly Piece Empty = new Piece();

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

        internal int productid;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(productid), ref productid);
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
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            if (productid == 0) s.PutNull(nameof(productid));
            else s.Put(nameof(productid), productid);
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
        }

        public int Key => id;
    }
}