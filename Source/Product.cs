using SkyChain;

namespace Revital
{
    /// <summary>
    /// A product data model.
    /// </summary>
    public class Product : _Article, IKeyable<int>
    {
        public static readonly Product Empty = new Product();


        internal short id;

        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
        }

        public int Key => id;
    }
}