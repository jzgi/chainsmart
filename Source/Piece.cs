using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for post entry.
    /// </summary>
    public class Piece : _Ware, IKeyable<int>
    {
        public static readonly Piece Empty = new Piece();

        internal int id;
        internal int productid;

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(productid), ref productid);
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
        }

        public int Key => id;
    }
}