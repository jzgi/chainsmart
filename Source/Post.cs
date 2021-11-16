using SkyChain;

namespace Revital
{
    /// 
    /// A data model for biz post.
    /// 
    public class Post : _Ware, IKeyable<int>
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

            s.Put(nameof(planid), planid);
        }

        public int Key => id;
    }
}