using SkyChain;

namespace Revital
{
    /// <summary>
    /// A data model for post entry.
    /// </summary>
    public class Post : _Art, IKeyable<int>
    {
        public static readonly Post Empty = new Post();

        internal int id;
        internal int planid;

        public override void Read(ISource s, byte proj = 0x0f)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(planid), ref planid);
        }

        public override void Write(ISink s, byte proj = 0x0f)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            if (planid == 0) s.PutNull(nameof(planid)); else s.Put(nameof(planid), planid);
        }

        public int Key => id;
    }
}