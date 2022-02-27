using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for post entry.
    /// </summary>
    public class Piece : Info, IKeyable<int>
    {
        public static readonly Piece Empty = new Piece();

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
        internal int id;
        internal int productid;

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
            s.Get(nameof(productid), ref productid);
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
            if (productid == 0) s.PutNull(nameof(productid));
            else s.Put(nameof(productid), productid);
        }

        public int Key => id;
    }
}