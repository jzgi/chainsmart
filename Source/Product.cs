using SkyChain;

namespace Revital
{
    /// <summary>
    /// The data modal for a yield of item.
    /// </summary>
    public class Product : _Doc, IKeyable<int>
    {
        public static readonly Product Empty = new Product();


        internal short id;
        internal short itemid;


        public override void Read(ISource s, byte proj = 15)
        {
            base.Read(s, proj);
            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }

            s.Get(nameof(itemid), ref itemid);
        }

        public override void Write(ISink s, byte proj = 15)
        {
            base.Write(s, proj);
            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }

            s.Put(nameof(itemid), itemid);
        }

        public int Key => id;
    }
}