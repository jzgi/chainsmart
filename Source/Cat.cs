using CoChain;

namespace Revital
{
    /// <summary>
    /// The data modal for an standard category.
    /// </summary>
    public class Cat : Entity, IKeyable<short>, IDirectory
    {
        public static readonly Cat Empty = new Cat();

        internal short idx;

        internal short num; // number of items

        // must have an icon

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            s.Get(nameof(idx), ref idx);
            s.Get(nameof(num), ref num);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            s.Put(nameof(idx), idx);
            s.Put(nameof(num), num);
        }

        public short Key => typ;

        public override string ToString() => name;

        public short Idx => idx;

        public short Num => num;
    }
}