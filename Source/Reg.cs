using Chainly;

namespace Revital
{
    public class Reg : Info, IKeyable<short>
    {
        public static readonly Reg Empty = new Reg();

        public const short
            TYP_PROV = 1,
            TYP_MRT_DIV = 2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_PROV, "省份"},
            {TYP_MRT_DIV, "市场区划"},
        };

        internal short id;
        internal short idx;
        internal short num; // number of resources contained

        public override void Read(ISource s, short proj = 0xff)
        {
            base.Read(s, proj);

            if ((proj & ID) == ID)
            {
                s.Get(nameof(id), ref id);
            }
            s.Get(nameof(idx), ref idx);
            s.Get(nameof(num), ref num);
        }

        public override void Write(ISink s, short proj = 0xff)
        {
            base.Write(s, proj);

            if ((proj & ID) == ID)
            {
                s.Put(nameof(id), id);
            }
            s.Put(nameof(idx), idx);
            s.Put(nameof(num), num);
        }

        public short Key => id;

        public bool IsProv => typ == TYP_PROV;

        public bool IsMrtDiv => typ == TYP_MRT_DIV;

        public override string ToString() => name;
    }
}