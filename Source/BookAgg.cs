using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class BookAgg : IData, IKeyable<int>
    {
        public static readonly BookAgg Empty = new BookAgg();

        internal string name; // shop

        internal int shpid; // shop
        internal int shpname;
        internal int itemid; // shop
        internal int count;
        internal short status;

        public void Read(ISource s, short msk = 0xff)
        {
            s.Get(nameof(name), ref name);
            s.Get(nameof(shpid), ref shpid);
            s.Get(nameof(shpname), ref shpname);
            s.Get(nameof(count), ref count);
            s.Get(nameof(status), ref status);
        }

        public void Write(ISink s, short msk = 0xff)
        {
        }

        public int Key => shpid;
    }
}