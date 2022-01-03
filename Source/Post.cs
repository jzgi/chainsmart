using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for post entry.
    /// </summary>
    public class Post : _Article, IKeyable<int>
    {
        public static readonly Post Empty = new Post();

        public const short
            INSERT = TYP | STATUS | LABEL | CREATE | ID | BASIC,
            UPDATE = STATUS | LABEL | ADAPT | BASIC;


        internal int id;
        internal int productid;

        public override void Read(ISource s, short proj = 0x0fff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(productid), ref productid);
        }

        public override void Write(ISink s, short proj = 0x0fff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if (productid == 0) s.PutNull(nameof(productid)); else s.Put(nameof(productid), productid);
        }

        public int Key => id;
    }
}